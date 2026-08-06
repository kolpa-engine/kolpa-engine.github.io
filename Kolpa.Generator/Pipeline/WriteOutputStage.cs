using System.Text;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that writes rendered routing streams to disk using the file system abstraction.
/// </summary>
public class WriteOutputStage : IBuildStage
{
    private readonly IFileSystem _fileSystem;

    public string Name => "Write Output";

    public WriteOutputStage(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;
        var outputDir = Path.GetFullPath(Path.Combine(root, context.Config.Paths.Output));

        // 1. Wipe Output folder
        try
        {
            _fileSystem.DeleteDirectory(outputDir, true);
            _fileSystem.CreateDirectory(outputDir);
        }
        catch (Exception ex)
        {
            context.AddDiagnostic(DiagnosticSeverity.Error, $"Failed to clean output folder {outputDir}: {ex.Message}", Name);
            return;
        }

        // 2. Write Rendered Pages
        foreach (var route in context.Routes)
        {
            if (string.IsNullOrEmpty(route.RenderedHtml))
            {
                continue;
            }

            try
            {
                await _fileSystem.WriteFileAsync(route.OutputPath, route.RenderedHtml);

                var bytesCount = Encoding.UTF8.GetByteCount(route.RenderedHtml);
                context.GeneratedFiles.Add(new GeneratedFile(route.OutputPath, bytesCount));
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(DiagnosticSeverity.Error, $"Failed to write output route {route.Url} to disk: {ex.Message}", Name);
            }
        }

        context.AddDiagnostic(DiagnosticSeverity.Info, $"Wrote {context.GeneratedFiles.Count} pages to {outputDir}", Name);
    }
}
