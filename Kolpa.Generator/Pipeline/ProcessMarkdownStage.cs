using System.Text.Json;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Renders raw Markdown document bodies to HTML via the configured <c>IMarkdownRenderer</c>.
/// Results are cached by content hash plus a Markdown configuration signature.
/// </summary>
public class ProcessMarkdownStage(IMarkdownRenderer renderer, ICacheService cache, ILogger logger)
    : IBuildStage
{
    private readonly IMarkdownRenderer _renderer = renderer;
    private readonly ICacheService _cache = cache;
    private readonly ILogger _logger = logger;

    public string Name => "Process Markdown";

    public Task ExecuteAsync(BuildContext context)
    {
        var configSignature = JsonSerializer.Serialize(context.Config.Markdown);
        int processed = 0;
        int cacheHits = 0;

        foreach (var doc in context.Documents)
        {
            if (!doc.Format.Equals("markdown", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var html = RenderCached(doc.Body, configSignature, ref cacheHits);
            doc.Body = html;
            processed++;

            _logger.LogVerbose($"[Markdown] {doc.Id} processed");
        }

        _logger.LogInfo(
            $"[Markdown] Rendered {processed} markdown document(s)"
                + (cacheHits > 0 ? $" ({cacheHits} cached)" : string.Empty)
        );

        context.AddDiagnostic(
            DiagnosticSeverity.Info,
            $"Processed markdown for {processed} document(s) ({cacheHits} cache hits).",
            Name
        );

        return Task.CompletedTask;
    }

    private string RenderCached(string markdown, string configSignature, ref int cacheHits)
    {
        if (_cache.Enabled)
        {
            var key = _cache.ComputeHash(configSignature + markdown);
            if (_cache.TryReadText(key, "markdown", out var cached))
            {
                cacheHits++;
                return cached;
            }

            var html = _renderer.Render(markdown);
            _cache.StoreText(key, "markdown", html);
            return html;
        }

        return _renderer.Render(markdown);
    }
}
