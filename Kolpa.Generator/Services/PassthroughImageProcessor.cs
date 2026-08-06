using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Pass-through image processor used when the <c>assets.images.processor</c> is set to
/// "passthrough". It preserves originals and performs no resizing or format conversion.
/// </summary>
public class PassthroughImageProcessor : IImageProcessor
{
    private static readonly string[] _supported =
    [
        "png",
        "jpg",
        "jpeg",
        "webp",
        "gif",
        "avif",
        "svg",
    ];

    public bool CanProcess(string extension)
    {
        return _supported.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public Task<ImageDimensions> IdentifyAsync(
        string sourcePath,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(new ImageDimensions { Width = 0, Height = 0 });
    }

    public async Task<ImageProcessingResult> ProcessAsync(
        string sourcePath,
        ImageProcessOptions options,
        CancellationToken cancellationToken = default
    )
    {
        var bytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
        var ext = Path.GetExtension(sourcePath).TrimStart('.').ToLowerInvariant();

        var result = new ImageProcessingResult
        {
            SourcePath = sourcePath,
            OriginalWidth = 0,
            OriginalHeight = 0,
            Variants =
            {
                new ImageVariant
                {
                    FileName = $"{Path.GetFileNameWithoutExtension(sourcePath)}.{ext}",
                    Format = ext,
                    Width = 0,
                    Height = 0,
                    SizeBytes = bytes.Length,
                    IsOriginal = true,
                    Content = bytes,
                },
            },
        };

        return result;
    }
}
