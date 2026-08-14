using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kolpa.Generator.Services;

/// <summary>
/// A lightweight development HTTP server for serving local static website builds with Live-Reload.
/// Uses TcpListener instead of HttpListener to avoid admin/URL-reservation requirements on Windows.
/// </summary>
public class DevServer(string serveDir, int port = 5000, string host = "localhost")
{
    private static readonly ConcurrentBag<StreamWriter> _sseClients = [];
    private readonly string _serveDir = Path.GetFullPath(serveDir);
    private readonly int _port = port;
    private readonly string _host = host;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Broadcasts a reload event to all active EventSource clients.
    /// </summary>
    public static void BroadcastReload()
    {
        var deadClients = new ConcurrentBag<StreamWriter>();
        var payload = "data: reload\n\n";

        Parallel.ForEach(
            _sseClients,
            client =>
            {
                try
                {
                    client.Write(payload);
                    client.Flush();
                }
                catch
                {
                    deadClients.Add(client);
                }
            }
        );

        if (!deadClients.IsEmpty)
        {
            var active = new ConcurrentBag<StreamWriter>();
            foreach (var client in _sseClients)
            {
                if (!deadClients.Contains(client))
                    active.Add(client);
                else
                    try
                    {
                        client.Dispose();
                    }
                    catch { }
            }
            _sseClients.Clear();
            foreach (var client in active)
                _sseClients.Add(client);
        }
    }

    /// <summary>
    /// Starts the server in a background task.
    /// </summary>
    public void Start()
    {
        if (!Directory.Exists(_serveDir))
            Directory.CreateDirectory(_serveDir);

        var bindAddr = ResolveBindAddress(_host);
        _listener = new TcpListener(bindAddr, _port);
        _listener.Start();

        _cts = new CancellationTokenSource();

        var displayHost = _host == "0.0.0.0" || _host == "+" ? "+" : _host;
        var lanIp = GetLanIpAddress();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SERVE] Dev server started at http://{displayHost}:{_port}/");
        if (lanIp != null && _host != lanIp)
            Console.WriteLine($"[SERVE] Network access: http://{lanIp}:{_port}/");
        Console.WriteLine($"[SERVE] Serving files from: {_serveDir}");
        Console.ResetColor();

        Task.Run(() => AcceptLoopAsync(_cts.Token));
    }

    private static IPAddress ResolveBindAddress(string host)
    {
        if (host == "localhost" || host == "127.0.0.1" || host == "::1")
            return IPAddress.Loopback;
        if (host == "0.0.0.0" || host == "+")
            return IPAddress.Any;
        if (IPAddress.TryParse(host, out var parsed))
            return parsed;
        // Try DNS resolve
        var addrs = Dns.GetHostAddresses(host);
        return addrs.Length > 0 ? addrs[0] : IPAddress.Any;
    }

    private static string? GetLanIpAddress()
    {
        try
        {
            var hostname = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostname);
            foreach (var addr in addresses)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(addr))
                    return addr.ToString();
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Stops the server listener loop.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();

        foreach (var client in _sseClients)
            try
            {
                client.Dispose();
            }
            catch { }
        _sseClients.Clear();

        Console.WriteLine("[SERVE] Dev server stopped.");
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        if (_listener == null)
            return;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                _ = HandleConnectionAsync(tcpClient, token);
            }
            catch (SocketException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVE ERROR] {ex.Message}");
            }
        }
    }

    private async Task HandleConnectionAsync(TcpClient tcpClient, CancellationToken token)
    {
        using var _ = tcpClient;
        try
        {
            using var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
            {
                AutoFlush = true,
            };

            // Read the HTTP request line
            var requestLine = await reader.ReadLineAsync(token);
            if (string.IsNullOrEmpty(requestLine))
                return;

            var parts = requestLine.Split(' ');
            if (parts.Length < 2)
                return;

            var method = parts[0];
            var rawPath = parts[1];

            // Read remaining headers (we don't need them for a static server, but must consume them)
            string? line;
            while ((line = await reader.ReadLineAsync(token)) != null && line.Length > 0)
            {
                // skip headers
            }

            var urlPath = WebUtility.UrlDecode(rawPath);

            // SSE endpoint for live reload
            if (urlPath.Equals("/__livereload", StringComparison.OrdinalIgnoreCase))
            {
                await WriteSseHeaders(writer);
                await writer.WriteAsync("data: connected\n\n");
                await writer.FlushAsync(token);

                _sseClients.Add(writer);

                // Keep connection open, wait for cancellation
                try
                {
                    while (!token.IsCancellationRequested)
                        await Task.Delay(Timeout.Infinite, token);
                }
                catch (OperationCanceledException) { }
                return;
            }

            // Serve static files
            if (urlPath.StartsWith("/"))
                urlPath = urlPath.Substring(1);

            var localPath = Path.Combine(_serveDir, urlPath);

            // Clean URLs: directory -> index.html
            if (Directory.Exists(localPath))
                localPath = Path.Combine(localPath, "index.html");
            else if (!File.Exists(localPath) && File.Exists(localPath + ".html"))
                localPath = localPath + ".html";

            if (File.Exists(localPath))
            {
                var bytes = await File.ReadAllBytesAsync(localPath, token);
                var contentType = GetContentType(localPath);
                await WriteHttpResponse(writer, 200, "OK", contentType, bytes);
            }
            else
            {
                var custom404Path = Path.Combine(_serveDir, "404.html");
                if (File.Exists(custom404Path))
                {
                    var bytes = await File.ReadAllBytesAsync(custom404Path, token);
                    await WriteHttpResponse(writer, 404, "Not Found", "text/html", bytes);
                }
                else
                {
                    var body = Encoding.UTF8.GetBytes("<h1>404 Not Found</h1>");
                    await WriteHttpResponse(writer, 404, "Not Found", "text/html", body);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVE ERROR] {ex.Message}");
        }
    }

    private static async Task WriteHttpResponse(
        StreamWriter writer,
        int status,
        string statusText,
        string contentType,
        byte[] body
    )
    {
        await writer.WriteAsync($"HTTP/1.1 {status} {statusText}\r\n");
        await writer.WriteAsync($"Content-Type: {contentType}\r\n");
        await writer.WriteAsync($"Content-Length: {body.Length}\r\n");
        await writer.WriteAsync("Connection: close\r\n");
        await writer.WriteAsync("\r\n");
        await writer.FlushAsync();

        // Write raw bytes through the underlying stream
        var stream = writer.BaseStream;
        await stream.WriteAsync(body);
        await stream.FlushAsync();
    }

    private static async Task WriteSseHeaders(StreamWriter writer)
    {
        await writer.WriteAsync("HTTP/1.1 200 OK\r\n");
        await writer.WriteAsync("Content-Type: text/event-stream\r\n");
        await writer.WriteAsync("Cache-Control: no-cache\r\n");
        await writer.WriteAsync("Connection: keep-alive\r\n");
        await writer.WriteAsync("\r\n");
        await writer.FlushAsync();
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
            ".woff2" => "font/woff2",
            ".woff" => "font/woff",
            ".ttf" => "font/ttf",
            ".xml" => "application/xml",
            ".txt" => "text/plain",
            _ => "application/octet-stream",
        };
    }
}
