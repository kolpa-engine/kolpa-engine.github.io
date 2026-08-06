namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Highlights source code into HTML using CSS classes (never inline styles).
/// Providers are pluggable and selected through configuration.
/// </summary>
public interface ICodeHighlighter
{
    /// <summary>
    /// Returns highlighted HTML for the given raw source and language identifier,
    /// or <c>null</c> when the language is not supported (caller falls back to plain output).
    /// </summary>
    string? Highlight(string code, string language);

    /// <summary>
    /// True when this provider can process the given language identifier.
    /// </summary>
    bool Supports(string language);
}
