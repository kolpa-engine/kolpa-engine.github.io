using System.Xml.Linq;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kolpa.Generator.Services;

/// <summary>
/// Post-build plugin that generates an RSS 2.0 feed from a configured collection.
/// </summary>
public class RssPlugin : IEnginePlugin, IBuildStep
{
    private GeneratorConfig? _config;

    public string Name => "RSS Feed Generator Plugin";

    public void ConfigureServices(IServiceCollection services, GeneratorConfig config)
    {
        _config = config;
        services.AddSingleton<IBuildStep>(this);
    }

    public async Task ExecuteAsync(SiteContext siteContext, string outputDir)
    {
        var config = _config;
        if (config == null || !config.Rss.Enabled)
        {
            return;
        }

        var baseUrl = config.Site.Url.TrimEnd('/');
        var collectionName = config.Rss.Collection.ToLowerInvariant();

        if (!siteContext.Collections.TryGetValue(collectionName, out var posts))
        {
            return;
        }

        var title = config.Site.Title;
        var description = config.Site.Description;
        var link = baseUrl + EnsureLeadingSlash(config.Rss.Link);

        var ns = XNamespace.Get("http://www.w3.org/2005/Atom");
        var rss = new XElement(
            "rss",
            new XAttribute("version", "2.0"),
            new XAttribute(XNamespace.Xml + "ns", ns),
            new XElement(
                "channel",
                new XElement("title", title),
                new XElement("link", link),
                new XElement("description", description),
                new XElement("language", "en-us"),
                posts.Select(post =>
                {
                    var url =
                        (post.TryGetValue("url", out var urlVal) ? urlVal?.ToString() : null)
                        ?? config.Rss.Link;
                    var itemTitle =
                        (post.TryGetValue("title", out var titleVal) ? titleVal?.ToString() : null)
                        ?? "Untitled";
                    var itemDescription = post.TryGetValue("description", out var descVal)
                        ? descVal?.ToString() ?? ""
                        : "";
                    var content = post.TryGetValue("content", out var contentVal)
                        ? contentVal?.ToString() ?? ""
                        : "";
                    var pubDate =
                        post.TryGetValue("date", out var dateVal) && dateVal is DateTime dt
                            ? dt.ToUniversalTime().ToString("R")
                            : DateTime.UtcNow.ToString("R");

                    var item = new XElement(
                        "item",
                        new XElement("title", itemTitle),
                        new XElement("link", baseUrl + EnsureLeadingSlash(url)),
                        new XElement("guid", baseUrl + EnsureLeadingSlash(url)),
                        new XElement("pubDate", pubDate),
                        new XElement("description", itemDescription),
                        new XElement("encoded", content, new XAttribute(ns + "type", "html"))
                    );

                    if (post.TryGetValue("author", out var authorVal) && authorVal != null)
                    {
                        item.Add(new XElement("author", authorVal.ToString()));
                    }

                    return item;
                })
            )
        );

        var feedPath = Path.Combine(outputDir, config.Rss.Output);
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null), rss);
        await File.WriteAllTextAsync(feedPath, document.ToString());
    }

    private static string EnsureLeadingSlash(string url)
    {
        return url.StartsWith('/') ? url : "/" + url;
    }
}
