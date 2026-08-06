namespace Kolpa.Generator.Models;

/// <summary>
/// A unified model representing a page, markdown file, or static site collection document.
/// Supports a content graph where documents reference other documents.
/// </summary>
public class ContentDocument
{
    /// <summary>
    /// Unique identifier for the document (typically relative path or slug).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The collection type (e.g. "page", "blog", "docs").
    /// </summary>
    public string Type { get; set; } = "page";

    /// <summary>
    /// Source content format ("markdown" or "liquid"). Determines whether the body
    /// is run through the Markdown renderer during the processing pipeline.
    /// </summary>
    public string Format { get; set; } = "liquid";

    /// <summary>
    /// Strongly-typed metadata parsed from the document frontmatter.
    /// </summary>
    public ContentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Rendered HTML or template body markup content.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// File paths or slugs referencing other documents in the content graph.
    /// </summary>
    public List<string> References { get; set; } = new();

    /// <summary>
    /// Clean URL path slug (e.g. "blog/first-post").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Final generated URL path (e.g. "/blog/first-post/index.html").
    /// </summary>
    public string OutputUrl { get; set; } = string.Empty;
}
