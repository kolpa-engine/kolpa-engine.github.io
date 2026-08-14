using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Parser interface that decodes file contents into a ContentDocument structure.
/// </summary>
public interface IContentParser
{
    /// <summary>
    /// Checks if this parser can handle the specified file extension.
    /// </summary>
    bool CanParse(string fileExtension);

    /// <summary>
    /// Parses a file path into a unified ContentDocument model.
    /// </summary>
    Task<ContentDocument> ParseAsync(string filePath);
}
