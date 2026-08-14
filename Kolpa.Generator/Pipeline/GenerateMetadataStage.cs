using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Scans image assets, reads intrinsic dimensions, and registers responsive image
/// metadata so templates can build <c>picture</c>/<c>img srcset</c> markup. No image
/// files are written here; that happens in <c>ProcessImagesStage</c>.
/// </summary>
public class GenerateMetadataStage(IImageProcessor imageProcessor, ILogger logger) : IBuildStage
{
    private readonly IImageProcessor _imageProcessor = imageProcessor;
    private readonly ILogger _logger = logger;

    public string Name => "Generate Metadata";

    public async Task ExecuteAsync(BuildContext context)
    {
        var config = context.Config;
        var images = config.Assets.Images;

        context.Metadata["assets"] = BuildAssetIds(context);

        if (!images.Enabled)
        {
            return;
        }

        var root = context.Project.RootPath;
        var assetsSrc = Path.Combine(root, config.Paths.Assets);
        var registry = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        int registered = 0;

        foreach (var asset in context.Assets)
        {
            var ext = Path.GetExtension(asset).TrimStart('.').ToLowerInvariant();
            if (!images.Include.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!_imageProcessor.CanProcess(ext))
            {
                continue;
            }

            try
            {
                var rel = Path.GetRelativePath(assetsSrc, asset).Replace('\\', '/');
                var dimensions = await _imageProcessor.IdentifyAsync(asset);
                registry[rel] = BuildInfo(config, rel, dimensions, images);
                registered++;
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(
                    DiagnosticSeverity.Warning,
                    $"Failed to identify image metadata for {asset}: {ex.Message}",
                    Name
                );
            }
        }

        context.Metadata["images"] = registry;
        _logger.LogInfo($"[Images] Registered metadata for {registered} image(s).");

        context.Metadata["assets"] = BuildAssetIds(context);

        context.AddDiagnostic(
            DiagnosticSeverity.Info,
            $"Generated responsive metadata for {registered} image(s).",
            Name
        );
    }

    private Dictionary<string, object> BuildAssetIds(BuildContext context)
    {
        var config = context.Config;
        var assetsSrc = Path.Combine(context.Project.RootPath, config.Paths.Assets);
        var processing = config.Assets.Processing;
        var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(assetsSrc))
        {
            return map;
        }

        foreach (var asset in context.Assets)
        {
            var ext = Path.GetExtension(asset).TrimStart('.').ToLowerInvariant();
            if (processing.Enabled && (ext == "css" || ext == "js"))
            {
                var content = AssetFingerprint.MinifyContent(asset, ext, processing);
                var outputPath = AssetFingerprint.ResolveOutputPath(
                    Path.GetRelativePath(assetsSrc, asset).Replace('\\', '/'),
                    content,
                    processing
                );
                map[Path.GetRelativePath(assetsSrc, asset).Replace('\\', '/')] =
                    $"/{config.Paths.Assets}/{outputPath}";
            }
            else
            {
                var relative = Path.GetRelativePath(assetsSrc, asset).Replace('\\', '/');
                map[relative] = $"/{config.Paths.Assets}/{relative}";
            }
        }

        var highlight = config.Markdown.Highlighting;
        if (highlight.GenerateCss)
        {
            var cssFile = string.IsNullOrWhiteSpace(highlight.CssFile)
                ? "highlight.css"
                : highlight.CssFile;
            map[cssFile] = $"/{config.Paths.Assets}/{cssFile}";
        }

        return map;
    }

    private static Dictionary<string, object> BuildInfo(
        GeneratorConfig config,
        string relativePath,
        ImageDimensions dimensions,
        ImageSettings images
    )
    {
        var assetsPath = config.Paths.Assets;
        var dir = Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? string.Empty;
        var baseName = Path.GetFileNameWithoutExtension(relativePath);
        var urlBase =
            dir.Length > 0 ? $"{assetsPath}/{dir}/{baseName}" : $"{assetsPath}/{baseName}";

        var width = dimensions.Width;
        var height = dimensions.Height;
        var renderWidth = ImagePlan.MaxRenderWidth(images, width);
        var renderHeight = ImagePlan.ScaledHeight(width, height, renderWidth);

        var sources = new List<Dictionary<string, object>>();
        string src;

        if (images.GenerateWebP)
        {
            src = $"/{urlBase}.webp";
            sources.Add(
                new Dictionary<string, object>
                {
                    ["src"] = $"/{urlBase}.webp",
                    ["width"] = renderWidth,
                    ["format"] = "webp",
                }
            );

            foreach (var size in ImagePlan.ResponsiveWidths(images, width))
            {
                sources.Add(
                    new Dictionary<string, object>
                    {
                        ["src"] = $"/{urlBase}-{size}.webp",
                        ["width"] = size,
                        ["format"] = "webp",
                    }
                );
            }
        }
        else
        {
            src = $"/{assetsPath}/{relativePath}";
        }

        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["src"] = src,
            ["width"] = width,
            ["height"] = height,
            ["renderWidth"] = renderWidth,
            ["renderHeight"] = renderHeight,
            ["sources"] = sources,
            ["formats"] = images.GenerateWebP ? new List<string> { "webp" } : new List<string>(),
        };
    }
}
