using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that renders routing templates to intermediate strings using template context factories.
/// </summary>
public class RenderTemplatesStage(TemplateService templateService) : IBuildStage
{
    private readonly TemplateService _templateService = templateService;

    public string Name => "Render Templates";

    public async Task ExecuteAsync(BuildContext context)
    {
        // 1. Build SiteContext model
        var siteContext = SiteContextFactory.Create(context);

        // 1a. Build per-collection tag clouds ({ name, slug, count }), sorted by name.
        foreach (var collKvp in context.Collections)
        {
            var counts = collKvp
                .Value.SelectMany(doc => doc.Metadata.Tags)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .GroupBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count());

            var cloud = counts
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kvp =>
                {
                    var slug = System
                        .Text.RegularExpressions.Regex.Replace(
                            kvp.Key.Trim().ToLowerInvariant(),
                            "[^a-z0-9]+",
                            "-"
                        )
                        .Trim('-');
                    return new Dictionary<string, object>
                    {
                        ["name"] = kvp.Key,
                        ["slug"] = slug,
                        ["count"] = kvp.Value,
                    };
                })
                .ToList();

            siteContext.Tags[collKvp.Key] = cloud;
        }

        // render each route in parallel. rendering is safe because every route receives
        // its own clone of the page context. the shared site/data/collection maps are only
        // read during this phase.
        var routes = context.Routes.ToArray();
        var renderErrors = new System.Collections.Concurrent.ConcurrentBag<string>();

        Parallel.ForEach(
            routes,
            route =>
            {
                try
                {
                    var doc = new ContentDocument
                    {
                        Id = route.InputPath,
                        Body = route.Template,
                        Metadata = route.Metadata,
                        Slug = route.Url.Trim('/'),
                    };

                    var siteClone = CloneSiteContext(siteContext);
                    var renderedHtml = _templateService
                        .RenderPageAsync(doc, siteClone)
                        .GetAwaiter()
                        .GetResult();
                    route.RenderedHtml = renderedHtml;
                }
                catch (Exception ex)
                {
                    renderErrors.Add($"Failed to render route page '{route.Url}': {ex.Message}");
                }
            }
        );

        // diagnostics are reported after the parallel loop to keep ordering deterministic
        // and because BuildContext.Diagnostics is not safe for concurrent writes.
        foreach (var route in routes)
        {
            if (!string.IsNullOrEmpty(route.RenderedHtml))
            {
                context.AddDiagnostic(
                    DiagnosticSeverity.Info,
                    $"Successfully rendered route page: {route.Url}",
                    Name
                );
            }
        }

        foreach (var error in renderErrors)
        {
            context.AddDiagnostic(DiagnosticSeverity.Error, error, Name);
        }
    }

    /// <summary>
    /// Creates an independent copy of <see cref="SiteContext"/> sharing the read-only maps,
    /// so parallel renders can safely assign their own <c>Page</c> metadata.
    /// </summary>
    private static SiteContext CloneSiteContext(SiteContext source)
    {
        return new SiteContext
        {
            Site = source.Site,
            Data = source.Data,
            Collections = source.Collections,
            Tags = source.Tags,
            Images = source.Images,
            Assets = source.Assets,
            Urls = source.Urls,
        };
    }
}
