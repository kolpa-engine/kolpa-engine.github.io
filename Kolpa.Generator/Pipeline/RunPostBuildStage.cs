using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that triggers registered post-build tasks and plugins.
/// </summary>
public class RunPostBuildStage : IBuildStage
{
    private readonly IEnumerable<IBuildStep> _postBuildSteps;

    public string Name => "Run Post-Build";

    public RunPostBuildStage(IEnumerable<IBuildStep> postBuildSteps)
    {
        _postBuildSteps = postBuildSteps;
    }

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
