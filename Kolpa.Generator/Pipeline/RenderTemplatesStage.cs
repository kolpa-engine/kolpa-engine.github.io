using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that renders routing templates to intermediate strings using template context factories.
/// </summary>
public class RenderTemplatesStage : IBuildStage
{
    private readonly TemplateService _templateService;

    public string Name => "Render Templates";

    public RenderTemplatesStage(TemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task ExecuteAsync(BuildContext context)
    {
        // 1. Build SiteContext model
        var siteContext = new SiteContext();
        siteContext.Site["title"] = context.Config.Site.Title;
        siteContext.Site["description"] = context.Config.Site.Description;
        siteContext.Site["url"] = context.Config.Site.Url;

        siteContext.Urls = context
            .Routes.Select(r => r.Url)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(u => u, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var dataKvp in context.DataRegistry)
        {
            siteContext.Data[dataKvp.Key] = dataKvp.Value;
        }

        foreach (var collKvp in context.Collections)
        {
            var rawList = collKvp
                .Value.OrderByDescending(doc => doc.Metadata.Date ?? DateTime.MinValue)
                .Select(doc =>
                {
                    var dict = doc.Metadata.ToDictionary();
                    dict["content"] = doc.Body;
                    dict["slug"] = doc.Slug;
                    dict["url"] = doc.OutputUrl;
                    return dict;
                })
                .ToList();

            siteContext.Collections[collKvp.Key] = rawList;
        }

        // 1b. Build per-collection tag clouds ({ name, slug, count }), sorted by name.
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

        // 2. Render each route in queue
        foreach (var route in context.Routes)
        {
            try
            {
                // Reconstruct a temporary ContentDocument to map metadata to template rendering
                var doc = new ContentDocument
                {
                    Id = route.InputPath,
                    Body = route.Template,
                    Metadata = route.Metadata,
                    Slug = route.Url.Trim('/'),
                };

                var renderedHtml = await _templateService.RenderPageAsync(doc, siteContext);
                route.RenderedHtml = renderedHtml;

                context.AddDiagnostic(
                    DiagnosticSeverity.Info,
                    $"Successfully rendered route page: {route.Url}",
                    Name
                );
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(
                    DiagnosticSeverity.Error,
                    $"Failed to render route page '{route.Url}': {ex.Message}",
                    Name
                );
            }
        }
    }
}
