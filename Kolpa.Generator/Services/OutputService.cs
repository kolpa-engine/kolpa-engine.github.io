using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// Service managing directory wipes, creation, and writing rendered documents to disk.
/// </summary>
public class OutputService(ILogger logger)
{
    private readonly ILogger _logger = logger;

  /// <summary>
  /// Clears target directory.
  /// </summary>
  public void CleanDirectory(string outputDir)
    {
        try
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
                _logger.LogVerbose($"Deleted directory: {outputDir}");
            }
            Directory.CreateDirectory(outputDir);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed cleaning directory {outputDir}: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes content body asynchronously, creating folders if needed.
    /// Retries on IOException (file locked by dev server reading it).
    /// </summary>
    public async Task WriteFileAsync(string physicalPath, string content)
    {
        var dir = Path.GetDirectoryName(physicalPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        for (int attempt = 0; ; attempt++)
        {
            try
            {
                await File.WriteAllTextAsync(physicalPath, content);
                _logger.LogVerbose($"Generated output file: {physicalPath}");
                return;
            }
            catch (IOException) when (attempt < 5)
            {
                await Task.Delay(100 * (attempt + 1));
            }
        }
    }
}
