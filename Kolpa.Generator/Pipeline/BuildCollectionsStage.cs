using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that filters drafts, groups documents by collections, and populates registries.
/// </summary>
public class BuildCollectionsStage : IBuildStage
{
    public string Name => "Build Collections";

    public Task ExecuteAsync(BuildContext context)
    {
        // 1. Filter out draft documents
        var activeDocs = context.Documents.Where(doc => !doc.Metadata.Draft).ToList();
        context.Documents.Clear();
        context.Documents.AddRange(activeDocs);

        // 2. Group by Type and populate Collections map
        foreach (var doc in activeDocs)
        {
            var collName = doc.Type.ToLowerInvariant();
            if (collName == "page")
            {
                continue; // Pages do not belong to looped collection groups by default
            }

            if (!context.Collections.TryGetValue(collName, out var list))
            {
                list = new List<ContentDocument>();
                context.Collections[collName] = list;
            }
            list.Add(doc);
        }

        // 3. Log collection summary
        foreach (var kvp in context.Collections)
        {
            context.AddDiagnostic(DiagnosticSeverity.Info, $"Built collection '{kvp.Key}' with {kvp.Value.Count} items", Name);
        }

        return Task.CompletedTask;
    }
}
