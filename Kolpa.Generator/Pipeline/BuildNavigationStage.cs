using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that builds the dynamic navigation links tree from route definitions.
/// </summary>
public class BuildNavigationStage : IBuildStage
{
    public string Name => "Build Navigation";

    public Task ExecuteAsync(BuildContext context)
    {
        // Discover navigation items from metadata configuration parameters
        var navRoutes = context.Routes
            .Where(r => r.Metadata["nav"] != null || r.Metadata["order"] != null)
            .OrderBy(r => Convert.ToInt32(r.Metadata["order"] ?? 0))
            .ToList();

        foreach (var route in navRoutes)
        {
            var title = route.Metadata.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = Path.GetFileNameWithoutExtension(route.InputPath);
            }

            var node = new NavigationNode
            {
                Title = title,
                Url = route.Url,
                Order = Convert.ToInt32(route.Metadata["order"] ?? 0)
            };

            context.Navigation.Add(node);
            context.AddDiagnostic(DiagnosticSeverity.Info, $"Added navigation node: '{node.Title}' -> {node.Url}", Name);
        }

        return Task.CompletedTask;
    }
}
