namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Renders Markdown source into HTML using a configurable set of extensions.
/// Implementations stay free of site-specific concerns and are selected via configuration.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Converts Markdown source text to an HTML fragment.
    /// </summary>
    string Render(string markdown);
}
