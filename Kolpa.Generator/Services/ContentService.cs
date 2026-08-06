using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Service managing content files discovery, parser mapping, drafts filtering, and collection groups.
/// </summary>
public class ContentService(IContentProvider contentProvider, ILogger logger, string projectDir)
{
    private readonly IContentProvider _contentProvider = contentProvider;
    private readonly ILogger _logger = logger;
    private readonly string _projectDir = projectDir;

  /// <summary>
  /// Loads the pages content documents.
  /// </summary>
  public async Task<IEnumerable<ContentDocument>> LoadPagesAsync(string pagesSource)
    {
        var sourcePath = Path.Combine(_projectDir, pagesSource);
        var pages = await _contentProvider.LoadContentAsync(sourcePath);

        // Filter out drafts
        return pages.Where(p => !p.Metadata.Draft);
    }

    /// <summary>
    /// Loads global JSON data files registry.
    /// </summary>
    public async Task<Dictionary<string, object>> LoadGlobalDataAsync(string dataSource)
    {
        var dataRegistry = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var sourcePath = Path.Combine(_projectDir, dataSource);

        if (!Directory.Exists(sourcePath))
        {
            return dataRegistry;
        }

        var files = Directory.GetFiles(sourcePath, "*.json");
        foreach (var file in files)
        {
            var key = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            try
            {
                var jsonContent = await File.ReadAllTextAsync(file);
                var deserialized = JsonSerializer.Deserialize<object>(jsonContent);
                if (deserialized != null)
                {
                    dataRegistry[key] = deserialized;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarn($"Failed to deserialize global data file {file}: {ex.Message}");
            }
        }

        return dataRegistry;
    }

    /// <summary>
    /// Discovers and loads content collections dynamically based on configuration settings.
    /// </summary>
    public async Task<Dictionary<string, List<ContentDocument>>> LoadCollectionsAsync(Dictionary<string, CollectionSettings> collectionsConfig)
    {
        var collections = new Dictionary<string, List<ContentDocument>>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in collectionsConfig)
        {
            var name = kvp.Key.ToLowerInvariant();
            var settings = kvp.Value;
            var sourcePath = Path.Combine(_projectDir, settings.Source);

            if (!Directory.Exists(sourcePath))
            {
                _logger.LogVerbose($"Source path for collection '{name}' does not exist: {sourcePath}. Skipping.");
                continue;
            }

            var items = await _contentProvider.LoadContentAsync(sourcePath);
            var filteredItems = items
                .Where(doc => !doc.Metadata.Draft)
                .Select(doc =>
                {
                    doc.Type = name;
                    return doc;
                })
                .ToList();

            collections[name] = filteredItems;
        }

        return collections;
    }
}
