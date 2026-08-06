using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that triggers registered post-build tasks and plugins.
/// </summary>
public class RunPostBuildStage(IEnumerable<IBuildStep> postBuildSteps) : IBuildStage
{
    private readonly IEnumerable<IBuildStep> _postBuildSteps = postBuildSteps;

    public string Name => "Run Post-Build";

    public async Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;
        var outputDir = Path.GetFullPath(Path.Combine(root, context.Config.Paths.Output));

        // Reconstruct context variables if needed, then execute each registered build step
        foreach (var step in _postBuildSteps)
        {
            try
            {
                // Reconstruct a simplified SiteContext from data
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

                if (
                    context.Metadata.TryGetValue("images", out var imagesObj)
                    && imagesObj is Dictionary<string, object> images
                )
                {
                    foreach (var imageKvp in images)
                    {
                        siteContext.Images[imageKvp.Key] = imageKvp.Value;
                    }
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

                await step.ExecuteAsync(siteContext, outputDir);
                context.AddDiagnostic(
                    DiagnosticSeverity.Info,
                    $"Executed build step plugin: {step.GetType().Name}",
                    Name
                );
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(
                    DiagnosticSeverity.Warning,
                    $"Build step plugin {step.GetType().Name} failed: {ex.Message}",
                    Name
                );
            }
        }
    }
}
