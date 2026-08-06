using System.Text;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Copies non-image static assets (css, js, fonts, video, svg) into the output and emits
/// the generated syntax-highlighting stylesheet when enabled.
/// </summary>
public class OptimizeAssetsStage(IImageProcessor imageProcessor, ILogger logger) : IBuildStage
{
    private readonly IImageProcessor _imageProcessor = imageProcessor;
    private readonly ILogger _logger = logger;

    public string Name => "Optimize Assets";

    public Task ExecuteAsync(BuildContext context)
    {
        var config = context.Config;
        var root = context.Project.RootPath;
        var assetsSrc = Path.Combine(root, config.Paths.Assets);
        var assetsDest = Path.Combine(root, config.Paths.Output, config.Paths.Assets);
        var images = config.Assets.Images;

        if (Directory.Exists(assetsSrc))
        {
            int copied = 0;
            foreach (var asset in context.Assets)
            {
                var ext = Path.GetExtension(asset).TrimStart('.').ToLowerInvariant();
                bool isProcessedImage =
                    images.Enabled
                    && images.Include.Contains(ext, StringComparer.OrdinalIgnoreCase)
                    && _imageProcessor.CanProcess(ext);

                if (isProcessedImage)
                {
                    continue;
                }

                try
                {
                    var relative = Path.GetRelativePath(assetsSrc, asset);
                    var dest = Path.Combine(assetsDest, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    File.Copy(asset, dest, true);
                    copied++;
                }
                catch (Exception ex)
                {
                    context.AddDiagnostic(
                        DiagnosticSeverity.Warning,
                        $"Failed copying asset {asset}: {ex.Message}",
                        Name
                    );
                }
            }

            _logger.LogVerbose($"[Assets] Copied {copied} static asset(s).");
        }

        WriteHighlightCss(context, assetsDest);

        context.AddDiagnostic(DiagnosticSeverity.Info, "Optimized and copied static assets.", Name);
        return Task.CompletedTask;
    }

    private void WriteHighlightCss(BuildContext context, string assetsDest)
    {
        var highlighting = context.Config.Markdown.Highlighting;
        if (!highlighting.GenerateCss)
        {
            return;
        }

        var theme = HighlightTheme.ResolveTheme(highlighting);
        var css = HighlightTheme.GenerateCss(highlighting, theme);

        var cssFile = string.IsNullOrWhiteSpace(highlighting.CssFile)
            ? "highlight.css"
            : highlighting.CssFile;
        var dest = Path.Combine(assetsDest, cssFile);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.WriteAllText(dest, css, Encoding.UTF8);

        _logger.LogVerbose($"[Assets] Generated highlighting theme CSS: {cssFile}");
    }
}
