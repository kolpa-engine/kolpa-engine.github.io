using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Kolpa.Generator.Services;

/// <summary>
/// A lightweight development HTTP server for serving local static website builds with Live-Reload.
/// </summary>
public class DevServer(string serveDir, int port = 5000)
{
    private static readonly ConcurrentBag<HttpListenerResponse> _sseClients = new();
    private readonly string _serveDir = Path.GetFullPath(serveDir);
    private readonly int _port = port;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;

  /// <summary>
  /// Broadcasts a reload event to all active EventSource clients.
  /// </summary>
  public static void BroadcastReload()
    {
        var deadClients = new ConcurrentBag<HttpListenerResponse>();
        var payload = Encoding.UTF8.GetBytes("data: reload\n\n");

        Parallel.ForEach(_sseClients, client =>
        {
            try
            {
                client.OutputStream.Write(payload, 0, payload.Length);
                client.OutputStream.Flush();
            }
            catch
            {
                deadClients.Add(client);
            }
        });

        // Cleanup dead streams
        if (!deadClients.IsEmpty)
        {
            var active = new ConcurrentBag<HttpListenerResponse>();
            foreach (var client in _sseClients)
            {
                if (!deadClients.Contains(client))
                {
                    active.Add(client);
                }
                else
                {
                    try { client.OutputStream.Close(); } catch { }
                }
            }
            _sseClients.Clear();
            foreach (var client in active)
            {
                _sseClients.Add(client);
            }
        }
    }

    /// <summary>
    /// Starts the server in a background task.
    /// </summary>
    public void Start()
    {
        if (!Directory.Exists(_serveDir))
        {
            Directory.CreateDirectory(_serveDir);
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port}/");
        _listener.Start();

        _cts = new CancellationTokenSource();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SERVE] Dev server started at http://localhost:{_port}/");
        Console.WriteLine($"[SERVE] Serving files from: {_serveDir}");
        Console.ResetColor();

        Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    /// <summary>
    /// Stops the server listener loop.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();

        // Close all active SSE connections
        foreach (var client in _sseClients)
        {
            try { client.OutputStream.Close(); } catch { }
        }
        _sseClients.Clear();

        Console.WriteLine("[SERVE] Dev server stopped.");
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        if (_listener == null) return;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = ProcessRequestAsync(context);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVE ERROR] {ex.Message}");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var urlPath = WebUtility.UrlDecode(request.Url?.AbsolutePath ?? "/");

            // Expose Server-Sent Events endpoint for Live Reload
            if (urlPath.Equals("/__livereload", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentType = "text/event-stream";
                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Connection", "keep-alive");
                response.StatusCode = (int)HttpStatusCode.OK;

                // Send initial connection packet to establish stream
                var connectMsg = Encoding.UTF8.GetBytes("data: connected\n\n");
                await response.OutputStream.WriteAsync(connectMsg, 0, connectMsg.Length);
                await response.OutputStream.FlushAsync();

                _sseClients.Add(response);
                return; // Keep connection open
            }

            if (urlPath.StartsWith("/")) urlPath = urlPath.Substring(1);

            var localPath = Path.Combine(_serveDir, urlPath);

            // Clean URLs mapping
            if (Directory.Exists(localPath))
            {
                localPath = Path.Combine(localPath, "index.html");
            }
            else if (!File.Exists(localPath) && File.Exists(localPath + ".html"))
            {
                localPath = localPath + ".html";
            }

            if (File.Exists(localPath))
            {
                var bytes = await File.ReadAllBytesAsync(localPath);
                response.ContentType = GetContentType(localPath);
                response.ContentLength64 = bytes.Length;
                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                var custom404Path = Path.Combine(_serveDir, "404.html");
                byte[] errorBytes;
                if (File.Exists(custom404Path))
                {
                    errorBytes = await File.ReadAllBytesAsync(custom404Path);
                    response.ContentType = "text/html";
                }
                else
                {
                    errorBytes = Encoding.UTF8.GetBytes("<h1>404 Not Found</h1><p>The requested static page could not be resolved.</p>");
                    response.ContentType = "text/html";
                }
                response.ContentLength64 = errorBytes.Length;
                await response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVE ERROR] Failed to process request: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            // Close normal HTTP requests, keeping SSE open
            if (request.Url?.AbsolutePath != "/__livereload")
            {
                try { response.OutputStream.Close(); } catch { }
            }
        }
    }

    private static string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
