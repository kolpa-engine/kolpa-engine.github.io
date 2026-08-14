using Kolpa.Generator.Config;

namespace Kolpa.Generator.Services;

/// <summary>
/// Centralizes the output naming rules for processed images so metadata generation,
/// caching, and the processor stages always agree on produced file names.
/// </summary>
public static class ImagePlan
{
    /// <summary>Largest render width for a source image honoring the configured max width.</summary>
    public static int MaxRenderWidth(ImageSettings cfg, int originalWidth)
    {
        return cfg.MaxWidth > 0 && originalWidth > cfg.MaxWidth ? cfg.MaxWidth : originalWidth;
    }

    public static int ScaledHeight(int originalWidth, int originalHeight, int targetWidth)
    {
        if (originalWidth <= 0)
        {
            return originalHeight;
        }

        return Math.Max(1, (int)Math.Round(targetWidth * (double)originalHeight / originalWidth));
    }

    /// <summary>Responsive widths smaller than the source (never upscales).</summary>
    public static IEnumerable<int> ResponsiveWidths(ImageSettings cfg, int originalWidth)
    {
        var target = MaxRenderWidth(cfg, originalWidth);
        foreach (var size in cfg.Sizes.Distinct().OrderBy(s => s))
        {
            if (size > 0 && size < target)
            {
                yield return size;
            }
        }
    }

    /// <summary>
    /// The complete set of output file names that will be produced for a source image.
    /// Used to skip processing when every variant is already cached.
    /// </summary>
    public static List<string> ComputeVariantFileNames(
        string relativePath,
        int width,
        int height,
        ImageSettings cfg
    )
    {
        var baseName = Path.GetFileNameWithoutExtension(relativePath);
        var ext = Path.GetExtension(relativePath).TrimStart('.').ToLowerInvariant();
        var names = new List<string>();

        if (cfg.PreserveOriginal)
        {
            names.Add($"{baseName}.{ext}");
        }

        if (cfg.GenerateWebP)
        {
            names.Add($"{baseName}.webp");
            foreach (var size in ResponsiveWidths(cfg, width))
            {
                names.Add($"{baseName}-{size}.webp");
            }
        }

        if (cfg.GenerateAvif)
        {
            names.Add($"{baseName}.avif");
        }

        if (!cfg.GenerateWebP && !cfg.GenerateAvif && !cfg.PreserveOriginal)
        {
            names.Add($"{baseName}.{ext}");
        }

        return names;
    }
}
