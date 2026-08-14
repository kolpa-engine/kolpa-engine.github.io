using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that generates URL routing maps and detects collisions.
/// </summary>
public class ResolveRoutesStage(IRouteGenerator routeGenerator) : IBuildStage
{
    private readonly IRouteGenerator _routeGenerator = routeGenerator;

    public string Name => "Resolve Routes";

  public Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;
        var outputDir = Path.GetFullPath(Path.Combine(root, context.Config.Paths.Output));
        var routeMap = new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase);

        // 1. Resolve Routes for Pages
        var pages = context.Documents.Where(doc => doc.Type.Equals("page", StringComparison.OrdinalIgnoreCase));
        foreach (var page in pages)
        {
            var url = _routeGenerator.GenerateCleanUrl(page, string.Empty);
            var physicalPath = _routeGenerator.GetPhysicalOutputPath(outputDir, url);

            var route = new Route
            {
                InputPath = page.Id,
                OutputPath = physicalPath,
                Url = url,
                Template = page.Body,
                Metadata = page.Metadata
            };

            AddRoute(context, routeMap, route);
            page.OutputUrl = url;
        }

        // 2. Resolve Routes for Collection items
        foreach (var collKvp in context.Collections)
        {
            var collName = collKvp.Key;
            var docs = collKvp.Value;
            string pattern = string.Empty;

            if (context.Config.Collections.TryGetValue(collName, out var collSettings))
            {
                pattern = collSettings.Output;
            }

            foreach (var doc in docs)
            {
                var url = _routeGenerator.GenerateCleanUrl(doc, pattern);
                var physicalPath = _routeGenerator.GetPhysicalOutputPath(outputDir, url);

                var route = new Route
                {
                    InputPath = doc.Id,
                    OutputPath = physicalPath,
                    Url = url,
                    Template = doc.Body,
                    Metadata = doc.Metadata
                };

                AddRoute(context, routeMap, route);
                doc.OutputUrl = url;
            }
        }

        context.Routes.AddRange(routeMap.Values);
        context.AddDiagnostic(DiagnosticSeverity.Info, $"Resolved {context.Routes.Count} target output routes.", Name);

        return Task.CompletedTask;
    }

    private void AddRoute(BuildContext context, Dictionary<string, Route> routeMap, Route route)
    {
        if (routeMap.TryGetValue(route.Url, out var existing))
        {
            context.AddDiagnostic(DiagnosticSeverity.Warning,
                $"Route collision detected: clean URL '{route.Url}' is targeted by multiple files: '{route.InputPath}' and '{existing.InputPath}'", Name);
        }
        else
        {
            routeMap[route.Url] = route;
        }
    }
}
