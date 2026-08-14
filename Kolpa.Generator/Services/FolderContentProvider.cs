using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Discovers and loads content files from a local folder directory.
/// </summary>
public class FolderContentProvider(IEnumerable<IContentParser> parsers) : IContentProvider
{
    private readonly IEnumerable<IContentParser> _parsers = parsers;

  public async Task<IEnumerable<ContentDocument>> LoadContentAsync(string sourceRoot)
    {
        var items = new List<ContentDocument>();
        if (!Directory.Exists(sourceRoot))
        {
            return items;
        }

        var files = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            var parser = _parsers.FirstOrDefault(p => p.CanParse(ext));
            if (parser != null)
            {
                try
                {
                    var parsed = await parser.ParseAsync(file);

                    // Generate a relative slug path preserving subfolders (e.g. "blog/post-1")
                    var relativePath = Path.GetRelativePath(sourceRoot, file);
                    var cleanSlug = Path.ChangeExtension(relativePath, null)
                                         .Replace(Path.DirectorySeparatorChar, '/')
                                         .ToLowerInvariant();

                    parsed.Slug = cleanSlug;
                    parsed.Id = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                    items.Add(parsed);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error loading file {file}: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        return items;
    }
}
