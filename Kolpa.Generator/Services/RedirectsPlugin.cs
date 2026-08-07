using System.Web;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kolpa.Generator.Services;

/// <summary>
/// Post-build plugin generating redirect/alias pages and a fallback 404 page.
/// Redirects are emitted as small HTML documents using an immediate meta-refresh plus a
/// canonical link, so they work on any static host (GitHub Pages, CDNs, file servers)
/// without server-side rewrites.
/// </summary>
public class RedirectsPlugin : IEnginePlugin, IBuildStep
{
    private GeneratorConfig? _config;

    public string Name => "Redirects & 404 Plugin";

    public void ConfigureServices(IServiceCollection services, GeneratorConfig config)
    {
        _config = config;
        services.AddSingleton<IBuildStep>(this);
    }

    public async Task ExecuteAsync(SiteContext siteContext, string outputDir)
    {
        var config = _config;
        if (config == null)
        {
            return;
        }

        if (config.Redirects.Enabled && config.Redirects.Rules.Count > 0)
        {
            foreach (var rule in config.Redirects.Rules)
            {
                if (string.IsNullOrWhiteSpace(rule.From) || string.IsNullOrWhiteSpace(rule.To))
                {
                    continue;
                }
                WriteRedirect(outputDir, rule);
            }
        }

        if (config.NotFound.Enabled)
        {
            await WriteNotFound(config, outputDir);
        }
    }

    private void WriteRedirect(string outputDir, RedirectRule rule)
    {
        var target = rule.To;
        if (!target.StartsWith("http://") && !target.StartsWith("https://"))
        {
            target = target.StartsWith('/') ? target : "/" + target;
        }

        var status = rule.Permanent ? "301" : "302";
        var title = "Redirecting...";
        var html =
            "<!doctype html>\n<html lang=\"en\">\n<head>\n"
            + "<meta charset=\"utf-8\">\n"
            + $"<title>{title}</title>\n"
            + $"<meta http-equiv=\"refresh\" content=\"0; url={HttpUtility.HtmlEncode(target)}\">\n"
            + $"<link rel=\"canonical\" href=\"{HttpUtility.HtmlEncode(target)}\">\n"
            + $"<script>location.replace({System.Text.Json.JsonSerializer.Serialize(target)});</script>\n"
            + "</head>\n<body>\n"
            + $"<p>Redirecting to <a href=\"{HttpUtility.HtmlEncode(target)}\">{HttpUtility.HtmlEncode(target)}</a>.</p>\n"
            + "</body>\n</html>\n";

        var path = ResolveOutputPath(outputDir, rule.From);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, html);
    }

    private async Task WriteNotFound(GeneratorConfig config, string outputDir)
    {
        if (string.IsNullOrWhiteSpace(config.NotFound.Output))
        {
            return;
        }

        var path = Path.IsPathRooted(config.NotFound.Output)
            ? config.NotFound.Output
            : Path.Combine(outputDir, config.NotFound.Output);

        if (File.Exists(path))
        {
            return;
        }

        var title = config.NotFound.Title;
        var body = config.NotFound.Body;
        var html =
            "<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n"
            + $"<title>{HttpUtility.HtmlEncode(title)}</title>\n"
            + "<meta name=\"robots\" content=\"noindex\">\n"
            + "</head>\n<body>\n"
            + body
            + "\n</body>\n</html>\n";

        await File.WriteAllTextAsync(path, html);
    }

    private static string ResolveOutputPath(string outputDir, string from)
    {
        var normalized = from.Trim();
        if (!normalized.StartsWith("/"))
        {
            normalized = "/" + normalized;
        }

        if (normalized.EndsWith("/") || !Path.HasExtension(normalized))
        {
            return Path.Combine(outputDir, normalized.Trim('/'), "index.html");
        }
        return Path.Combine(outputDir, normalized.Trim('/'));
    }
}
