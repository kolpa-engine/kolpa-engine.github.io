using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Kolpa.Generator.Services;

/// <summary>
/// Image processor backed by ImageSharp. Produces resized, compressed, and converted
/// variants (WebP, and AVIF when the underlying library exposes it). AVIF is invoked
/// via reflection so the processor works with ImageSharp versions that lack it.
/// </summary>
public class ImageSharpProcessor : IImageProcessor
{
    private static readonly string[] _supported = ["png", "jpg", "jpeg", "webp"];

    public bool CanProcess(string extension)
    {
        return _supported.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ImageDimensions> IdentifyAsync(
        string sourcePath,
        CancellationToken cancellationToken = default
    )
    {
        var info = await Image.IdentifyAsync(sourcePath, cancellationToken);
        return new ImageDimensions { Width = info.Width, Height = info.Height };
    }

    public async Task<ImageProcessingResult> ProcessAsync(
        string sourcePath,
        ImageProcessOptions options,
        CancellationToken cancellationToken = default
    )
    {
        var result = new ImageProcessingResult { SourcePath = sourcePath };

        using (var image = await Image.LoadAsync(sourcePath, cancellationToken))
        {
            result.OriginalWidth = image.Width;
            result.OriginalHeight = image.Height;

            var ext = Path.GetExtension(sourcePath).TrimStart('.').ToLowerInvariant();
            var dir = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(sourcePath);

            var targetWidth =
                options.MaxWidth > 0 && image.Width > options.MaxWidth
                    ? options.MaxWidth
                    : image.Width;
            var ratio = (double)targetWidth / image.Width;
            var targetHeight = Math.Max(1, (int)Math.Round(image.Height * ratio));

            // 1. Preserve an untouched original as the safe fallback <img>.
            if (options.PreserveOriginal)
            {
                var originalBytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
                result.Variants.Add(
                    new ImageVariant
                    {
                        FileName = $"{name}.{ext}",
                        Format = ext,
                        Width = image.Width,
                        Height = image.Height,
                        SizeBytes = originalBytes.Length,
                        IsOriginal = true,
                        Content = originalBytes,
                    }
                );
            }

            // 2. Primary encoded variant (modern format) at the max render width.
            if (options.GenerateWebP)
            {
                var bytes = await EncodeScaledAsync(
                    image,
                    targetWidth,
                    "webp",
                    options.Quality,
                    cancellationToken
                );
                result.Variants.Add(
                    new ImageVariant
                    {
                        FileName = $"{name}.webp",
                        Format = "webp",
                        Width = targetWidth,
                        Height = targetHeight,
                        SizeBytes = bytes.Length,
                        Content = bytes,
                    }
                );
            }

            if (options.GenerateAvif)
            {
                try
                {
                    var bytes = await EncodeScaledAsync(
                        image,
                        targetWidth,
                        "avif",
                        options.Quality,
                        cancellationToken
                    );
                    result.Variants.Add(
                        new ImageVariant
                        {
                            FileName = $"{name}.avif",
                            Format = "avif",
                            Width = targetWidth,
                            Height = targetHeight,
                            SizeBytes = bytes.Length,
                            Content = bytes,
                        }
                    );
                }
                catch (NotSupportedException)
                {
                    // AVIF encoder not available in the linked ImageSharp build; skip gracefully.
                }
            }

            // 3. Responsive downscaled variants in the primary modern format.
            if (options.GenerateWebP && options.Sizes is { Count: > 0 })
            {
                foreach (var size in options.Sizes.Distinct().OrderBy(s => s))
                {
                    if (size <= 0 || size >= targetWidth)
                    {
                        continue;
                    }

                    var sw = size;
                    var sh = Math.Max(
                        1,
                        (int)Math.Round(size * (double)image.Height / image.Width)
                    );
                    var bytes = await EncodeScaledAsync(
                        image,
                        sw,
                        "webp",
                        options.Quality,
                        cancellationToken
                    );
                    result.Variants.Add(
                        new ImageVariant
                        {
                            FileName = $"{name}-{sw}.webp",
                            Format = "webp",
                            Width = sw,
                            Height = sh,
                            SizeBytes = bytes.Length,
                            Content = bytes,
                        }
                    );
                }
            }

            // 4. No modern format and no preserved original: re-encode the source format.
            if (!options.GenerateWebP && !options.GenerateAvif && !options.PreserveOriginal)
            {
                var bytes = await EncodeScaledAsync(
                    image,
                    targetWidth,
                    ext == "jpg" ? "jpeg" : ext,
                    options.Quality,
                    cancellationToken
                );
                result.Variants.Add(
                    new ImageVariant
                    {
                        FileName = $"{name}.{ext}",
                        Format = ext,
                        Width = targetWidth,
                        Height = targetHeight,
                        SizeBytes = bytes.Length,
                        Content = bytes,
                    }
                );
            }
        }

        return result;
    }

    private static async Task<byte[]> EncodeScaledAsync(
        Image image,
        int width,
        string format,
        int quality,
        CancellationToken cancellationToken
    )
    {
        using var clone = image.Clone(x =>
            x.Resize(new ResizeOptions { Size = new Size(width, 0), Mode = ResizeMode.Max })
        );

        using var ms = new MemoryStream();
        switch (format)
        {
            case "webp":
                await clone.SaveAsWebpAsync(
                    ms,
                    new WebpEncoder { Quality = quality },
                    cancellationToken
                );
                break;
            case "png":
                await clone.SaveAsPngAsync(ms, cancellationToken);
                break;
            case "jpeg":
            case "jpg":
                await clone.SaveAsJpegAsync(
                    ms,
                    new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = quality },
                    cancellationToken
                );
                break;
            case "avif":
                await AvifEncoderHelper.SaveAsync(clone, ms, quality, cancellationToken);
                break;
            default:
                throw new NotSupportedException($"Unsupported image format: {format}");
        }

        return ms.ToArray();
    }
}
