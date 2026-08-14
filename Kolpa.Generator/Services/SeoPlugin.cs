using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kolpa.Generator.Services;

/// <summary>
/// Post-build plugin generating SEO artifacts: robots.txt, Atom feed, JSON Feed, and
/// JSON-LD structured data injected into every generated HTML page.
/// </summary>
public class SeoPlugin : IEnginePlugin, IBuildStep
{
    private GeneratorConfig? _config;

    public string Name => "SEO Generator Plugin";

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

        await WriteRobotsTxt(config, outputDir);

        await GenerateAtomFeed(config, siteContext, outputDir);
        await GenerateJsonFeed(config, siteContext, outputDir);

        if (config.Seo.JsonLd.Enabled)
        {
            await InjectJsonLdAsync(config, outputDir);
        }
    }

    private async Task WriteRobotsTxt(GeneratorConfig config, string outputDir)
    {
        if (!config.Seo.Robots.Enabled)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");

        foreach (var rule in config.Seo.Robots.Rules)
        {
            sb.AppendLine(rule);
        }

        if (config.Seo.Robots.IncludeSitemap && !string.IsNullOrWhiteSpace(config.Site.Url))
        {
            sb.AppendLine($"Sitemap: {config.Site.Url.TrimEnd('/')}/sitemap.xml");
        }

        var path = Path.Combine(outputDir, config.Seo.Robots.Output);
        await File.WriteAllTextAsync(path, sb.ToString());
    }

    private async Task GenerateAtomFeed(
        GeneratorConfig config,
        SiteContext siteContext,
        string outputDir
    )
    {
        if (!config.Atom.Enabled)
        {
            return;
        }

        var baseUrl = config.Site.Url.TrimEnd('/');
        var collection = config.Atom.Collection.ToLowerInvariant();
        if (!siteContext.Collections.TryGetValue(collection, out var posts))
        {
            return;
        }

        var feedLink = baseUrl + EnsureLeadingSlash(config.Atom.Link);
        var atom = XNamespace.Get("http://www.w3.org/2005/Atom");
        var feed = new XElement(
            atom + "feed",
            new XAttribute("xmlns", "http://www.w3.org/2005/Atom"),
            new XElement(atom + "title", config.Site.Title),
            new XElement(atom + "id", feedLink),
            new XElement(atom + "link", new XAttribute("href", feedLink), new XAttribute("rel", "alternate")),
            new XElement(atom + "updated", posts.Select(GetPostDate).DefaultIfEmpty(DateTime.UtcNow).Max().ToUniversalTime().ToString("o")),
            new XElement(atom + "author",
                new XElement(atom + "name", config.Site.Title),
                new XElement(atom + "uri", baseUrl)),
            posts.Select(post =>
            {
                var url = GetPostValue(post, "url", config.Atom.Link);
                var title = GetPostValue(post, "title", "Untitled");
                var content = GetPostValue(post, "content", "");
                var pubDate = GetPostDate(post);

                return new XElement(
                    atom + "entry",
                    new XElement(atom + "title", title),
                    new XElement(atom + "id", baseUrl + EnsureLeadingSlash(url)),
                    new XElement(atom + "link", new XAttribute("href", baseUrl + EnsureLeadingSlash(url))),
                    new XElement(atom + "updated", pubDate.ToUniversalTime().ToString("o")),
                    new XElement(atom + "published", pubDate.ToUniversalTime().ToString("o")),
                    new XElement(atom + "content", new XAttribute("type", "html"), content)
                );
            })
        );

        var path = Path.Combine(outputDir, config.Atom.Output);
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null), feed);
        await File.WriteAllTextAsync(path, document.ToString());
    }

    private async Task GenerateJsonFeed(
        GeneratorConfig config,
        SiteContext siteContext,
        string outputDir
    )
    {
        if (!config.JsonFeed.Enabled)
        {
            return;
        }

        var baseUrl = config.Site.Url.TrimEnd('/');
        var collection = config.JsonFeed.Collection.ToLowerInvariant();
        if (!siteContext.Collections.TryGetValue(collection, out var posts))
        {
            return;
        }

        var items = posts
            .Select(post => new
            {
                id = baseUrl + EnsureLeadingSlash(GetPostValue(post, "url", config.JsonFeed.Link)),
                url = baseUrl + EnsureLeadingSlash(GetPostValue(post, "url", config.JsonFeed.Link)),
                title = GetPostValue(post, "title", "Untitled"),
                content_html = GetPostValue(post, "content", ""),
                date_published = GetPostDate(post).ToUniversalTime().ToString("o"),
                summary = GetPostValue(post, "description", ""),
            })
            .ToList();

        var feed = new
        {
            version = "https://jsonfeed.org/version/1.1",
            title = config.Site.Title,
            home_page_url = baseUrl + "/",
            feed_url = baseUrl + EnsureLeadingSlash(config.JsonFeed.Output),
            description = config.Site.Description,
            items,
        };

        var path = Path.Combine(outputDir, config.JsonFeed.Output);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(feed, options));
    }

    private async Task InjectJsonLdAsync(GeneratorConfig config, string outputDir)
    {
        var htmlFiles = Directory.EnumerateFiles(outputDir, "*.html", SearchOption.AllDirectories);
        foreach (var file in htmlFiles)
        {
            var html = await File.ReadAllTextAsync(file);
            if (html.Contains("application/ld+json"))
            {
                continue;
            }

            var ldJson = BuildStructuredJson(config);
            var script =
                $"\n<script type=\"application/ld+json\">\n{ldJson}\n</script>\n";

            // Insert before </head> if present, otherwise append at end of <body>.
            var headEnd = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            var content = headEnd >= 0
                ? html.Insert(headEnd, script)
                : html + script;

            await File.WriteAllTextAsync(file, content);
        }
    }

    private static string BuildStructuredJson(GeneratorConfig config)
    {
        var jsonType = string.IsNullOrWhiteSpace(config.Seo.JsonLd.Type)
            ? "WebSite"
            : config.Seo.JsonLd.Type;

        var obj = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = jsonType,
            ["name"] = config.Site.Title,
            ["url"] = config.Site.Url,
            ["description"] = config.Site.Description,
        };

        if (!string.IsNullOrWhiteSpace(config.Seo.JsonLd.Image))
        {
            obj["image"] = config.Seo.JsonLd.Image;
        }
        else if (!string.IsNullOrWhiteSpace(config.Site.Url))
        {
            obj["image"] = $"{config.Site.Url.TrimEnd('/')}/assets/icon.png";
        }

        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        return JsonSerializer.Serialize(obj, options);
    }

    private static string GetPostValue(
        Dictionary<string, object> post,
        string key,
        string fallback
    )
    {
        return post.TryGetValue(key, out var val) && val != null ? val.ToString() ?? fallback : fallback;
    }

    private static DateTime GetPostDate(Dictionary<string, object> post)
    {
        return post.TryGetValue("date", out var val) && val is DateTime dt
            ? dt
            : DateTime.UtcNow;
    }

    private static string EnsureLeadingSlash(string url)
    {
        return url.StartsWith('/') ? url : "/" + url;
    }
}