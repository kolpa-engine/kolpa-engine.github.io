using System.Text.Json.Serialization;

namespace Kolpa.Generator.Config;

/// <summary>
/// Site metadata settings.
/// </summary>
public class SiteSettings
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Paths settings mapping to source directories.
/// </summary>
public class PathSettings
{
    [JsonPropertyName("pages")]
    public string Pages { get; set; } = "pages";

    [JsonPropertyName("layouts")]
    public string Layouts { get; set; } = "layouts";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "content";

    [JsonPropertyName("data")]
    public string Data { get; set; } = "data";

    [JsonPropertyName("assets")]
    public string Assets { get; set; } = "assets";

    [JsonPropertyName("output")]
    public string Output { get; set; } = "dist";
}

/// <summary>
/// Rendering configuration settings.
/// </summary>
public class RendererSettings
{
    [JsonPropertyName("engine")]
    public string Engine { get; set; } = "liquid";
}

/// <summary>
/// Root configuration file structure for the static site generator.
/// </summary>
public class GeneratorConfig
{
    [JsonPropertyName("site")]
    public SiteSettings Site { get; set; } = new();

    [JsonPropertyName("paths")]
    public PathSettings Paths { get; set; } = new();

    [JsonPropertyName("renderer")]
    public RendererSettings Renderer { get; set; } = new();

    [JsonPropertyName("collections")]
    public Dictionary<string, CollectionSettings> Collections { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Collection settings representing dynamic content groupings.
/// </summary>
public class CollectionSettings
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = "*.md";

    [JsonPropertyName("output")]
    public string Output { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string TagsOutput { get; set; } = string.Empty;
}
