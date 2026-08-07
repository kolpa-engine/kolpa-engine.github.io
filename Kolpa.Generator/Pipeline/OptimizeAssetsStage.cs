using System.Text;
using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Copies non-image static assets (css, js, fonts, video, svg) into the output and emits
/// the generated syntax-highlighting stylesheet when enabled. When asset processing is
/// enabled, CSS/JS files are minified and, when fingerprinting is enabled, emitted under a
/// content-hash name (e.g. <c>app.a1b2c3d4.css</c>) and recorded in a manifest. The
/// fingerprinted URL map is computed earlier in <c>GenerateMetadataStage</c> (so templates
/// can reference <c>{{ assets['app.css'] }}</c>); this stage writes files to match.
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
        var processing = config.Assets.Processing;

        var manifest = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(assetsSrc))
        {
            int copied = 0;
            int processed = 0;
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
                    var relative = Path.GetRelativePath(assetsSrc, asset).Replace('\\', '/');

                    if (processing.Enabled && (ext == "css" || ext == "js"))
                    {
                        var content = AssetFingerprint.MinifyContent(asset, ext, processing);
                        var outputPath = AssetFingerprint.ResolveOutputPath(
                            relative,
                            content,
                            processing
                        );
                        WriteToDisk(assetsDest, outputPath, content!);
                        manifest[relative] = $"/{config.Paths.Assets}/{outputPath}";
                        processed++;
                    }
                    else
                    {
                        var dest = Path.Combine(assetsDest, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                        File.Copy(asset, dest, true);
                        manifest[relative] = $"/{config.Paths.Assets}/{relative}";
                        copied++;
                    }
                }
                catch (Exception ex)
                {
                    context.AddDiagnostic(
                        DiagnosticSeverity.Warning,
                        $"Failed copying or processing asset {asset}: {ex.Message}",
                        Name
                    );
                }
            }

            _logger.LogVerbose(
                $"[Assets] Copied {copied} static asset(s), processed {processed} CSS/JS file(s)."
            );
        }

        var highlightRelative = WriteHighlightCss(context, assetsDest);
        if (highlightRelative != null)
        {
            manifest[highlightRelative] = $"/{config.Paths.Assets}/{highlightRelative}";
        }

        if (processing.Enabled && processing.Fingerprint && manifest.Count > 0)
        {
            WriteManifest(context, manifest);
        }

        context.AddDiagnostic(DiagnosticSeverity.Info, "Optimized and copied static assets.", Name);
        return Task.CompletedTask;
    }

    private static void WriteToDisk(string destDir, string outputPath, string content)
    {
        var dest = Path.Combine(destDir, outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.WriteAllText(dest, content, Encoding.UTF8);
    }

    private static void WriteManifest(BuildContext context, Dictionary<string, string> manifest)
    {
        var path = Path.Combine(
            context.Project.RootPath,
            context.Config.Paths.Output,
            context.Config.Assets.Processing.ManifestFile
        );
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(
            manifest,
            new JsonSerializerOptions { WriteIndented = true }
        );
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    public string? WriteHighlightCss(BuildContext context, string assetsDest)
    {
        var highlighting = context.Config.Markdown.Highlighting;
        if (!highlighting.GenerateCss)
        {
            return null;
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
        return cssFile;
    }
}
