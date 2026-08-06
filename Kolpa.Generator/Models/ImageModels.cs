namespace Kolpa.Generator.Models;

/// <summary>
/// Options controlling how the image processor produces output variants.
/// Populated from the configured <c>assets.images</c> section.
/// </summary>
public class ImageProcessOptions
{
    public bool Optimize { get; set; } = true;
    public bool GenerateWebP { get; set; } = true;
    public bool GenerateAvif { get; set; }
    public int Quality { get; set; } = 85;
    public int MaxWidth { get; set; } = 1920;
    public bool PreserveOriginal { get; set; } = true;
    public List<int> Sizes { get; set; } = new() { 320, 640, 1280, 1920 };
}

/// <summary>
/// Intrinsic dimensions of an image source.
/// </summary>
public class ImageDimensions
{
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// A single encoded image variant (a resized and/or converted output file).
/// </summary>
public class ImageVariant
{
    public string FileName { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long SizeBytes { get; set; }
    public bool IsOriginal { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Result of processing one source image into its responsive variants.
/// </summary>
public class ImageProcessingResult
{
    public string SourcePath { get; set; } = string.Empty;
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public List<ImageVariant> Variants { get; set; } = new();

    /// <summary>The variant that best represents the image (largest webp, else original).</summary>
    public ImageVariant PrimaryVariant =>
        Variants.FirstOrDefault(v => v.IsOriginal)
        ?? Variants.FirstOrDefault()
        ?? new ImageVariant();

    /// <summary>WebP variants ordered ascending by width, used to build a responsive source set.</summary>
    public List<ImageVariant> WebPVariants =>
        Variants
            .Where(v => v.Format.Equals("webp", StringComparison.OrdinalIgnoreCase))
            .OrderBy(v => v.Width)
            .ToList();
}
