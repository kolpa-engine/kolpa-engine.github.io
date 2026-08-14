using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Service that generates URL paths and output file locations for site documents.
/// </summary>
public interface IRouteGenerator
{
    /// <summary>
    /// Computes the final clean URL route slug and target output file path.
    /// </summary>
    /// <param name="document">The document being generated.</param>
    /// <param name="pattern">The routing pattern (e.g. "/blog/{slug}/" or "/{slug}.html").</param>
    /// <returns>A string containing the clean route path relative to the domain root.</returns>
    string GenerateCleanUrl(ContentDocument document, string pattern);

    /// <summary>
    /// Generates the absolute target output physical path on disk.
    /// </summary>
    string GetPhysicalOutputPath(string outputDir, string cleanUrl);
}
