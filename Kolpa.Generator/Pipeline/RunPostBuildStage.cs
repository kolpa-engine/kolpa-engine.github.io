using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

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
                // Reconstruct the SiteContext from the build context for each plugin.
                var siteContext = SiteContextFactory.Create(context);

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
