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

        foreach (var dataKvp in context.DataRegistry)
        {
            siteContext.Data[dataKvp.Key] = dataKvp.Value;
        }

        foreach (var collKvp in context.Collections)
        {
            var rawList = collKvp.Value.Select(doc =>
            {
                var dict = doc.Metadata.ToDictionary();
                dict["content"] = doc.Body;
                dict["slug"] = doc.Slug;
                return dict;
            }).ToList();

            siteContext.Collections[collKvp.Key] = rawList;
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
                    Slug = route.Url.Trim('/')
                };

                var renderedHtml = await _templateService.RenderPageAsync(doc, siteContext);
                route.RenderedHtml = renderedHtml;

                context.AddDiagnostic(DiagnosticSeverity.Info, $"Successfully rendered route page: {route.Url}", Name);
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(DiagnosticSeverity.Error, $"Failed to render route page '{route.Url}': {ex.Message}", Name);
            }
        }
    }
}
