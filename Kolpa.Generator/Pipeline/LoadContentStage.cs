using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that resolves content directories and parses documents using content parsers.
/// </summary>
public class LoadContentStage : IBuildStage
{
    private readonly IEnumerable<IContentParser> _parsers;
    private readonly IFileSystem _fileSystem;

    public string Name => "Load Content";

    public LoadContentStage(IEnumerable<IContentParser> parsers, IFileSystem fileSystem)
    {
        _parsers = parsers;
        _fileSystem = fileSystem;
    }

    public async Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;

        // 1. Load Main Pages
        var pagesPath = Path.Combine(root, context.Config.Paths.Pages);
        if (_fileSystem.DirectoryExists(pagesPath))
        {
            var pageFiles = _fileSystem.EnumerateFiles(pagesPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in pageFiles)
            {
                var doc = await ParseContentFileAsync(file, pagesPath);
                if (doc != null)
                {
                    doc.Type = "page";
                    context.Documents.Add(doc);
                }
            }
        }

        // 2. Load Collections source folders
        foreach (var kvp in context.Config.Collections)
        {
            var collName = kvp.Key;
            var settings = kvp.Value;
            var sourcePath = Path.Combine(root, settings.Source);

            if (!_fileSystem.DirectoryExists(sourcePath))
            {
                context.AddDiagnostic(DiagnosticSeverity.Warning, $"Collection '{collName}' source folder does not exist: {sourcePath}", Name);
                continue;
            }

            var pattern = settings.Pattern ?? "*.*";
            var collFiles = _fileSystem.EnumerateFiles(sourcePath, pattern, SearchOption.AllDirectories);
            foreach (var file in collFiles)
            {
                var doc = await ParseContentFileAsync(file, sourcePath);
                if (doc != null)
                {
                    doc.Type = collName;

                    // Prefix collection slug to avoid key collisions
                    doc.Slug = $"{collName}/{doc.Slug}";
                    context.Documents.Add(doc);
                }
            }
        }

        context.AddDiagnostic(DiagnosticSeverity.Info, $"Loaded {context.Documents.Count} documents.", Name);
    }

    private async Task<ContentDocument?> ParseContentFileAsync(string filePath, string rootFolder)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var parser = _parsers.FirstOrDefault(p => p.CanParse(ext));
        if (parser == null) return null;

        try
        {
            var doc = await parser.ParseAsync(filePath);
            var relative = Path.GetRelativePath(rootFolder, filePath);
            doc.Id = relative.Replace(Path.DirectorySeparatorChar, '/');

            // Set relative slug path preserving subfolders
            doc.Slug = Path.ChangeExtension(relative, null)
                           .Replace(Path.DirectorySeparatorChar, '/')
                           .ToLowerInvariant();

            return doc;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[PARSE ERROR] Failed to parse content file {filePath}: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
}
