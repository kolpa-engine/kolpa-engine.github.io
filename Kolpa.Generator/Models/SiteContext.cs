namespace Kolpa.Generator.Models;

/// <summary>
/// Data context passed to Liquid templates during page generation.
/// </summary>
public class SiteContext
{
    /// <summary>
    /// Site configuration settings (title, description).
    /// </summary>
    public Dictionary<string, object> Site { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Global JSON data registry (maps JSON file name to its structured data).
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Processed image metadata keyed by relative asset path. Each entry exposes
    /// <c>src</c>, <c>width</c>, <c>height</c>, and a <c>sources</c> list for building
    /// responsive <c>picture</c>/<c>img srcset</c> markup.
    /// </summary>
    public Dictionary<string, object> Images { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Processed asset URLs keyed by relative asset path. When fingerprinting is enabled
    /// each value is the content-hashed URL (e.g. <c>/assets/app.a1b2c3d4.css</c>); otherwise
    /// it is the plain asset URL. Templates use this to reference cache-busted files.
    /// </summary>
    public Dictionary<string, object> Assets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Content collections grouped by folder collection names (e.g., blog posts).
    /// </summary>
    public Dictionary<string, List<Dictionary<string, object>>> Collections { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Per-collection tag clouds: a sorted list of { name, slug, count } per collection name.
    /// </summary>
    public Dictionary<string, List<Dictionary<string, object>>> Tags { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Every output URL produced by the build (pages, collections, archives), used for sitemaps.
    /// </summary>
    public List<string> Urls { get; set; } = [];

    /// <summary>
    /// Current page metadata being rendered.
    /// </summary>
    public Dictionary<string, object> Page { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
