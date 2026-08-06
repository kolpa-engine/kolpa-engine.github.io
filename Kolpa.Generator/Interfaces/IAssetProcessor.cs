namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Handles static assets (styles, media, images) copy, bundling, and processing steps.
/// </summary>
public interface IAssetProcessor
{
    /// <summary>
    /// Processes all static assets in the source directory and writes them to the output directory.
    /// </summary>
    Task ProcessAssetsAsync(string sourceDir, string outputDir);
}
