using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that indexes and caches files from asset and layout directories.
/// </summary>
public class DiscoverFilesStage : IBuildStage
{
    private readonly IFileSystem _fileSystem;

    public string Name => "Discover Files";

    public DiscoverFilesStage(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;

        // 1. Discover Layout Templates
        var layoutsPath = Path.Combine(root, context.Config.Paths.Layouts);
        if (_fileSystem.DirectoryExists(layoutsPath))
        {
            var layoutFiles = _fileSystem.EnumerateFiles(layoutsPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in layoutFiles)
            {
                var name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                try
                {
                    var content = await _fileSystem.ReadFileAsync(file);
                    context.Templates[name] = content;
                    context.AddDiagnostic(DiagnosticSeverity.Info, $"Found template layout '{name}'", Name);
                }
                catch (Exception ex)
                {
                    context.AddDiagnostic(DiagnosticSeverity.Warning, $"Failed reading template layout {name}: {ex.Message}", Name);
                }
            }
        }

        // 2. Discover Asset files
        var assetsPath = Path.Combine(root, context.Config.Paths.Assets);
        if (_fileSystem.DirectoryExists(assetsPath))
        {
            var assetFiles = _fileSystem.EnumerateFiles(assetsPath, "*.*", SearchOption.AllDirectories);
            foreach (var asset in assetFiles)
            {
                context.Assets.Add(asset);
            }
            context.AddDiagnostic(DiagnosticSeverity.Info, $"Indexed {context.Assets.Count} static assets.", Name);
        }
    }
}
