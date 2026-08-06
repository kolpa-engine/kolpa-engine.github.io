using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// Implements asset processing and copying mechanisms.
/// </summary>
public class AssetService : IAssetProcessor
{
    private readonly ILogger _logger;

    public AssetService(ILogger logger)
    {
        _logger = logger;
    }

    public Task ProcessAssetsAsync(string sourceDir, string outputDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            _logger.LogVerbose($"Source assets directory does not exist: {sourceDir}. Skipping.");
            return Task.CompletedTask;
        }

        try
        {
            CopyDirectory(sourceDir, outputDir);
            _logger.LogVerbose($"Copied assets recursively from {sourceDir} to {outputDir}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to process static assets: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
            CopyDirectory(subDir, destSubDir);
        }
    }
}
