using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Optimizes a raster image into one or more output variants (resized, compressed,
/// converted formats). Implementations must be framework-agnostic at the caller level.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// True when this processor can handle the given source file extension (without dot, lower-case).
    /// </summary>
    bool CanProcess(string extension);

    /// <summary>
    /// Reads intrinsic image dimensions without performing a full optimization pass.
    /// Returns zero dimensions when the source cannot be decoded.
    /// </summary>
    Task<ImageDimensions> IdentifyAsync(
        string sourcePath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reads the source image and produces the requested output variants.
    /// </summary>
    /// <param name="sourcePath">Absolute path of the source image.</param>
    /// <param name="options">Current image processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImageProcessingResult> ProcessAsync(
        string sourcePath,
        ImageProcessOptions options,
        CancellationToken cancellationToken = default
    );
}
