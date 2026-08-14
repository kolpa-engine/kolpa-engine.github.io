using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Provider interface for loading and discovering collections of ContentDocuments.
/// </summary>
public interface IContentProvider
{
    /// <summary>
    /// Discovers and loads content items asynchronously from a source root directory.
    /// </summary>
    Task<IEnumerable<ContentDocument>> LoadContentAsync(string sourceRoot);
}
