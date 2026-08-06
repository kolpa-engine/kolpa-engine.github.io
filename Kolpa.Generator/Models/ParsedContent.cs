namespace Kolpa.Generator.Models;

/// <summary>
/// Represents parsed file contents containing metadata and renderable body content.
/// </summary>
public class ParsedContent
{
    /// <summary>
    /// File metadata parsed from frontmatter or configurations.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Renderable HTML or text body content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Clean URL routing path slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;
}
