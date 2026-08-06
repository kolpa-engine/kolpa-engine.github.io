using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Optimizes, resizes, and converts image assets into responsive variants, writing them
/// into the output assets directory. Results are cached by content hash + a configuration
/// signature so unchanged images are not reprocessed on rebuilds.
/// </summary>
public class ProcessImagesStage(IImageProcessor imageProcessor, ICacheService cache, ILogger logger)
    : IBuildStage
{
    private readonly IImageProcessor _imageProcessor = imageProcessor;
    private readonly ICacheService _cache = cache;
    private readonly ILogger _logger = logger;

    public string Name => "Process Images";

    public async Task ExecuteAsync(BuildContext context)
    {
        var config = context.Config;
        var images = config.Assets.Images;
        if (!images.Enabled)
        {
            _logger.LogVerbose("[Images] Disabled by configuration.");
            return;
        }

        var root = context.Project.RootPath;
        var assetsSrc = Path.Combine(root, config.Paths.Assets);
        var assetsDest = Path.Combine(root, config.Paths.Output, config.Paths.Assets);
        if (!Directory.Exists(assetsSrc))
        {
            return;
        }

        var options = new ImageProcessOptions
        {
            Optimize = images.Optimize,
            GenerateWebP = images.GenerateWebP,
            GenerateAvif = images.GenerateAvif,
            Quality = images.Quality,
            MaxWidth = images.MaxWidth,
            PreserveOriginal = images.PreserveOriginal,
            Sizes = images.Sizes,
        };

        var signature = JsonSerializer.Serialize(images) + images.Processor;
        var configKey = _cache.ComputeHash(signature);

        long totalSourceBytes = 0;
        long totalOutputBytes = 0;
        int processed = 0;
        int cacheHits = 0;

        foreach (var asset in context.Assets)
        {
            var ext = Path.GetExtension(asset).TrimStart('.').ToLowerInvariant();
            if (
                !images.Include.Contains(ext, StringComparer.OrdinalIgnoreCase)
                || !_imageProcessor.CanProcess(ext)
            )
            {
                continue;
            }

            try
            {
                var sourceBytes = File.ReadAllBytes(asset);
                var sourceHash = _cache.ComputeHash(sourceBytes);
                var manifestKey = _cache.ComputeHash(sourceHash + configKey);
                var rel = Path.GetRelativePath(assetsSrc, asset).Replace('\\', '/');

                var dimensions = await _imageProcessor.IdentifyAsync(asset);
                var expectedNames = ImagePlan.ComputeVariantFileNames(
                    rel,
                    dimensions.Width,
                    dimensions.Height,
                    images
                );

                ImageProcessingResult result;
                if (_cache.Enabled && TryReconstruct(manifestKey, expectedNames, out result))
                {
                    cacheHits++;
                }
                else
                {
                    result = await _imageProcessor.ProcessAsync(asset, options);
                    StoreManifest(manifestKey, expectedNames);
                    foreach (var variant in result.Variants)
                    {
                        _cache.StoreBytes(
                            CacheVariantKey(manifestKey, variant.FileName),
                            "images",
                            variant.Content
                        );
                    }
                    processed++;
                }

                foreach (var variant in result.Variants)
                {
                    var relDir = Path.GetDirectoryName(rel)?.Replace('\\', '/') ?? string.Empty;
                    var dest =
                        relDir.Length > 0
                            ? Path.Combine(assetsDest, relDir, variant.FileName)
                            : Path.Combine(assetsDest, variant.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    await File.WriteAllBytesAsync(dest, variant.Content);
                    totalOutputBytes += variant.SizeBytes;
                }

                totalSourceBytes += sourceBytes.Length;
                _logger.LogVerbose(
                    $"[Images] {rel} processed -> {result.Variants.Count} variant(s)"
                );
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(
                    DiagnosticSeverity.Warning,
                    $"Failed to process image {asset}: {ex.Message}",
                    Name
                );
            }
        }

        var saved = totalSourceBytes - totalOutputBytes;
        if (processed > 0)
        {
            _logger.LogInfo(
                $"[Images] Processed {processed} image(s) into responsive variants "
                    + $"{FormatBytes(totalSourceBytes)} -> {FormatBytes(totalOutputBytes)} "
                    + (saved > 0 ? $"(saved {FormatBytes(saved)})" : string.Empty)
            );
        }
        else if (cacheHits > 0)
        {
            _logger.LogInfo($"[Images] All {cacheHits} image(s) reused from cache.");
        }

        context.AddDiagnostic(
            DiagnosticSeverity.Info,
            $"Processed {processed} image(s) into responsive variants ({cacheHits} from cache).",
            Name
        );
    }

    private bool TryReconstruct(
        string manifestKey,
        List<string> expectedNames,
        out ImageProcessingResult result
    )
    {
        result = new ImageProcessingResult();
        if (!_cache.TryReadText(manifestKey, "images", out var manifestJson))
        {
            return false;
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<List<ImageVariant>>(manifestJson);
            if (manifest == null || manifest.Count == 0)
            {
                return false;
            }

            var variants = new List<ImageVariant>();
            foreach (var entry in manifest)
            {
                if (
                    !_cache.TryReadBytes(
                        CacheVariantKey(manifestKey, entry.FileName),
                        "images",
                        out var bytes
                    )
                )
                {
                    return false;
                }

                variants.Add(
                    new ImageVariant
                    {
                        FileName = entry.FileName,
                        Format = entry.Format,
                        Width = entry.Width,
                        Height = entry.Height,
                        IsOriginal = entry.IsOriginal,
                        SizeBytes = bytes.Length,
                        Content = bytes,
                    }
                );
            }

            result.Variants = variants;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void StoreManifest(string manifestKey, List<string> names)
    {
        if (!_cache.Enabled)
        {
            return;
        }

        var manifest = names
            .Select(name => new ImageVariant
            {
                FileName = name,
                Width = 0,
                Height = 0,
                Format = Path.GetExtension(name).TrimStart('.').ToLowerInvariant(),
            })
            .ToList();

        _cache.StoreText(manifestKey, "images", JsonSerializer.Serialize(manifest));
    }

    private static string CacheVariantKey(string manifestKey, string fileName)
    {
        return _cacheKeyPrefix + manifestKey + "-" + fileName;
    }

    private const string _cacheKeyPrefix = "img-";

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#}{units[unit]}";
    }
}
