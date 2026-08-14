using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// Highlighter that performs no tokenization. It simply preserves the language class
/// and HTML-escapes the code, which is useful when highlighting is disabled or as a
/// safe fallback provider.
/// </summary>
public class PassthroughCodeHighlighter : ICodeHighlighter
{
    public bool Supports(string language)
    {
        return true;
    }

    public string? Highlight(string code, string language)
    {
        return System.Net.WebUtility.HtmlEncode(code);
    }
}
