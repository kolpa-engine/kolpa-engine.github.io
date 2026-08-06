namespace Kolpa.Generator.Models;

/// <summary>
/// Data context passed to Liquid templates during page generation.
/// </summary>
public class SiteContext
{
    /// <summary>
    /// Site configuration settings (title, description).
    /// </summary>
    public Dictionary<string, object> Site { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Global JSON data registry (maps JSON file name to its structured data).
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Content collections grouped by folder collection names (e.g., blog posts).
    /// </summary>
    public Dictionary<string, List<Dictionary<string, object>>> Collections { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Current page metadata being rendered.
    /// </summary>
    public Dictionary<string, object> Page { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);
}
