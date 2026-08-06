using System.Xml.Linq;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kolpa.Generator.Services;

/// <summary>
/// Post-build plugin that generates an RSS 2.0 feed from the blog collection.
/// </summary>
public class RssPlugin : IEnginePlugin, IBuildStep
{
    private const string BaseUrl = "https://kolpa-engine.github.io";

    public string Name => "RSS Feed Generator Plugin";

    public void ConfigureServices(IServiceCollection services, GeneratorConfig config)
    {
        services.AddSingleton<IBuildStep>(this);
    }

    public async Task ExecuteAsync(SiteContext siteContext, string outputDir)
    {
        if (!siteContext.Collections.TryGetValue("blog", out var posts))
        {
            return;
        }

        var ns = XNamespace.Get("http://www.w3.org/2005/Atom");
        var rss = new XElement(
            "rss",
            new XAttribute("version", "2.0"),
            new XAttribute(XNamespace.Xml + "ns", ns),
            new XElement(
                "channel",
                new XElement("title", "Kolpa Engine Blog"),
                new XElement("link", BaseUrl + "/blog/"),
                new XElement(
                    "description",
                    "Development notes, tutorials, and updates from the Kolpa Engine team."
                ),
                new XElement("language", "en-us"),
                posts.Select(post =>
                {
                    var url =
                        (post.TryGetValue("url", out var urlVal) ? urlVal?.ToString() : null)
                        ?? "/blog/";
                    var title =
                        (post.TryGetValue("title", out var titleVal) ? titleVal?.ToString() : null)
                        ?? "Untitled";
                    var description = post.TryGetValue("description", out var descVal)
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
                        new XElement("title", title),
                        new XElement("link", BaseUrl + EnsureLeadingSlash(url)),
                        new XElement("guid", BaseUrl + EnsureLeadingSlash(url)),
                        new XElement("pubDate", pubDate),
                        new XElement("description", description),
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

        var feedPath = Path.Combine(outputDir, "feed.xml");
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null), rss);
        await File.WriteAllTextAsync(feedPath, document.ToString());
    }

    private static string EnsureLeadingSlash(string url)
    {
        return url.StartsWith('/') ? url : "/" + url;
    }
}
