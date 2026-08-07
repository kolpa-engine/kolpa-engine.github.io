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

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("showWarningBanner")]
    public bool ShowWarningBanner { get; set; } = false;

    [JsonPropertyName("warningBannerText")]
    public string WarningBannerText { get; set; } = string.Empty;
}

/// <summary>
/// RSS 2.0 feed generation settings for a single collection.
/// </summary>
public class RssSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("collection")]
    public string Collection { get; set; } = "blog";

    [JsonPropertyName("output")]
    public string Output { get; set; } = "feed.xml";

    [JsonPropertyName("link")]
    public string Link { get; set; } = "/";
}

/// <summary>
/// Atom feed generation settings for a single collection.
/// </summary>
public class AtomSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("collection")]
    public string Collection { get; set; } = "blog";

    [JsonPropertyName("output")]
    public string Output { get; set; } = "atom.xml";

    [JsonPropertyName("link")]
    public string Link { get; set; } = "/";
}

/// <summary>
/// JSON Feed generation settings (see https://www.jsonfeed.org) for a single collection.
/// </summary>
public class JsonFeedSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("collection")]
    public string Collection { get; set; } = "blog";

    [JsonPropertyName("output")]
    public string Output { get; set; } = "feed.json";

    [JsonPropertyName("link")]
    public string Link { get; set; } = "/";
}

/// <summary>
/// robots.txt generation settings.
/// </summary>
public class RobotsSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("output")]
    public string Output { get; set; } = "robots.txt";

    [JsonPropertyName("sitemap")]
    public bool IncludeSitemap { get; set; } = true;

    /// <summary>Explicit allow/disallow rules in the form of directives, e.g. "Disallow: /private/".</summary>
    [JsonPropertyName("rules")]
    public List<string> Rules { get; set; } = [];
}

/// <summary>
/// Structured data (JSON-LD) injection settings.
/// </summary>
public class JsonLdSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "WebSite";

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
}

/// <summary>
/// Search-engine optimization (SEO) settings.
/// </summary>
public class SeoSettings
{
    [JsonPropertyName("robots")]
    public RobotsSettings Robots { get; set; } = new();

    [JsonPropertyName("jsonLd")]
    public JsonLdSettings JsonLd { get; set; } = new();
}

/// <summary>
/// A single URL redirect rule (permanent 301 by default).
/// </summary>
public class RedirectRule
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("permanent")]
    public bool Permanent { get; set; } = true;
}

/// <summary>
/// URL redirect and alias generation settings. Each rule emits a lightweight HTML page
/// that performs a meta-refresh + canonical redirect to the target URL.
/// </summary>
public class RedirectSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("rules")]
    public List<RedirectRule> Rules { get; set; } = new();
}

/// <summary>
/// Custom 404 page generation settings. If a page resolving to <c>/404.html</c> already
/// exists it wins; otherwise a simple fallback page is emitted here.
/// </summary>
public class NotFoundSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("output")]
    public string Output { get; set; } = "404.html";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Page Not Found";

    [JsonPropertyName("body")]
    public string Body { get; set; } =
        "<h1>404</h1><p>The page you are looking for does not exist.</p>";
}

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
/// Markdown rendering and syntax highlighting settings.
/// </summary>
public class MarkdownSettings
{
    [JsonPropertyName("extensions")]
    public MarkdownExtensionsSettings Extensions { get; set; } = new();

    [JsonPropertyName("highlighting")]
    public HighlightingSettings Highlighting { get; set; } = new();
}

/// <summary>
/// Toggleable Markdig pipeline extensions. When <c>advanced</c> is enabled the
/// remaining flags are layered on top as fine-grained controls.
/// </summary>
public class MarkdownExtensionsSettings
{
    [JsonPropertyName("advanced")]
    public bool Advanced { get; set; } = true;

    [JsonPropertyName("tables")]
    public bool Tables { get; set; } = true;

    [JsonPropertyName("taskLists")]
    public bool TaskLists { get; set; } = true;

    [JsonPropertyName("footnotes")]
    public bool Footnotes { get; set; } = true;

    [JsonPropertyName("autoIdentifiers")]
    public bool AutoIdentifiers { get; set; } = true;

    [JsonPropertyName("strikethrough")]
    public bool Strikethrough { get; set; } = true;

    [JsonPropertyName("autoLinks")]
    public bool AutoLinks { get; set; } = true;

    [JsonPropertyName("definitionLists")]
    public bool DefinitionLists { get; set; } = false;

    [JsonPropertyName("emojiSmiles")]
    public bool EmojiSmiles { get; set; } = false;

    [JsonPropertyName("mathematics")]
    public bool Mathematics { get; set; } = false;
}

/// <summary>
/// Syntax highlighting configuration. <c>provider</c> selects the code highlighter
/// implementation ("builtin" or "passthrough"); <c>theme</c> is "light", "dark", or a
/// custom theme name resolved from <c>customTheme</c>.
/// </summary>
public class HighlightingSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "builtin";

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";

    [JsonPropertyName("cssPrefix")]
    public string CssPrefix { get; set; } = "hl-";

    [JsonPropertyName("generateCss")]
    public bool GenerateCss { get; set; } = true;

    [JsonPropertyName("cssFile")]
    public string CssFile { get; set; } = "highlight.css";

    [JsonPropertyName("customTheme")]
    public Dictionary<string, string> CustomTheme { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Image processing and responsive generation settings.
/// </summary>
public class ImageSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("processor")]
    public string Processor { get; set; } = "imagesharp";

    [JsonPropertyName("optimize")]
    public bool Optimize { get; set; } = true;

    [JsonPropertyName("generateWebP")]
    public bool GenerateWebP { get; set; } = true;

    [JsonPropertyName("generateAvif")]
    public bool GenerateAvif { get; set; } = false;

    [JsonPropertyName("quality")]
    public int Quality { get; set; } = 85;

    [JsonPropertyName("maxWidth")]
    public int MaxWidth { get; set; } = 1920;

    [JsonPropertyName("preserveOriginal")]
    public bool PreserveOriginal { get; set; } = true;

    [JsonPropertyName("sizes")]
    public List<int> Sizes { get; set; } = [320, 640, 1280, 1920];

    [JsonPropertyName("include")]
    public List<string> Include { get; set; } = ["png", "jpg", "jpeg", "webp"];
}

/// <summary>
/// Asset pipeline settings.
/// </summary>
public class AssetSettings
{
    [JsonPropertyName("images")]
    public ImageSettings Images { get; set; } = new();
}

/// <summary>
/// Incremental build caching settings.
/// </summary>
public class CacheSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("directory")]
    public string Directory { get; set; } = ".generator-cache";
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

    [JsonPropertyName("markdown")]
    public MarkdownSettings Markdown { get; set; } = new();

    [JsonPropertyName("assets")]
    public AssetSettings Assets { get; set; } = new();

    [JsonPropertyName("cache")]
    public CacheSettings Cache { get; set; } = new();

    [JsonPropertyName("rss")]
    public RssSettings Rss { get; set; } = new();

    [JsonPropertyName("atom")]
    public AtomSettings Atom { get; set; } = new();

    [JsonPropertyName("json")]
    public JsonFeedSettings JsonFeed { get; set; } = new();

    [JsonPropertyName("seo")]
    public SeoSettings Seo { get; set; } = new();

    [JsonPropertyName("redirects")]
    public RedirectSettings Redirects { get; set; } = new();

    [JsonPropertyName("notFound")]
    public NotFoundSettings NotFound { get; set; } = new();

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
