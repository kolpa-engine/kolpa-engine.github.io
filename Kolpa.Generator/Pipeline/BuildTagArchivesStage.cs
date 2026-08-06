using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that generates a clean URL route per tag for collections
/// configured with a "tags" output pattern (e.g. "/blog/tags/{tag}/").
/// </summary>
public class BuildTagArchivesStage : IBuildStage
{
    public string Name => "Build Tag Archives";

    public Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;
        var outputDir = Path.GetFullPath(Path.Combine(root, context.Config.Paths.Output));

        foreach (var collKvp in context.Config.Collections)
        {
            var collName = collKvp.Key.ToLowerInvariant();
            var pattern = collKvp.Value.TagsOutput;

            if (
                string.IsNullOrWhiteSpace(pattern)
                || !context.Collections.TryGetValue(collName, out var docs)
            )
            {
                continue;
            }

            var tags = docs.SelectMany(doc => doc.Metadata.Tags)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var tag in tags)
            {
                var tagSlug = SlugifyTag(tag);
                var resolved = pattern.Replace("{tag}", tagSlug);
                if (!resolved.StartsWith("/"))
                {
                    resolved = "/" + resolved;
                }

                var physicalPath = GetPhysicalOutputPath(outputDir, resolved);
                var route = new Route
                {
                    InputPath = $"{collName}/tags/{tagSlug}",
                    OutputPath = physicalPath,
                    Url = resolved,
                    Template = string.Empty,
                    Metadata = new ContentMetadata { Title = tag, Layout = "tag" },
                };
                route.Metadata["tag"] = tag;

                context.Routes.Add(route);
                context.AddDiagnostic(
                    DiagnosticSeverity.Info,
                    $"Generated tag archive '{tag}' -> {resolved}",
                    Name
                );
            }
        }

        return Task.CompletedTask;
    }

    private static string SlugifyTag(string tag)
    {
        var slug = System.Text.RegularExpressions.Regex.Replace(
            tag.Trim().ToLowerInvariant(),
            "[^a-z0-9]+",
            "-"
        );
        return slug.Trim('-');
    }

    private static string GetPhysicalOutputPath(string outputDir, string cleanUrl)
    {
        var normalized = cleanUrl.Trim('/');
        if (string.IsNullOrEmpty(normalized))
        {
            return Path.Combine(outputDir, "index.html");
        }
        return Path.Combine(outputDir, normalized, "index.html");
    }
}
