using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that copies static assets to the target output directory using the file system abstraction.
/// </summary>
public class ProcessAssetsStage(IFileSystem fileSystem) : IBuildStage
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public string Name => "Process Assets";

  public Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;
        var assetsSrc = Path.Combine(root, context.Config.Paths.Assets);
        var assetsDest = Path.Combine(root, context.Config.Paths.Output, context.Config.Paths.Assets);

        if (!_fileSystem.DirectoryExists(assetsSrc))
        {
            context.AddDiagnostic(DiagnosticSeverity.Warning, $"Source assets folder not found at: {assetsSrc}", Name);
            return Task.CompletedTask;
        }

        int count = 0;
        foreach (var file in context.Assets)
        {
            try
            {
                var relative = Path.GetRelativePath(assetsSrc, file);
                var destPath = Path.Combine(assetsDest, relative);

                _fileSystem.CopyFile(file, destPath, true);
                count++;
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(DiagnosticSeverity.Warning, $"Failed copying asset {file}: {ex.Message}", Name);
            }
        }

        context.AddDiagnostic(DiagnosticSeverity.Info, $"Successfully processed and copied {count} asset files.", Name);
        return Task.CompletedTask;
    }
}
