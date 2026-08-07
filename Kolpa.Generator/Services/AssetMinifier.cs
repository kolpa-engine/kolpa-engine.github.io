using System.Text;

namespace Kolpa.Generator.Services;

/// <summary>
/// Conservative, tokenizer-based CSS and JS minifiers. These never re-order tokens and
/// only strip comments/whitespace while preserving string and regex literals, so they are
/// safe for arbitrary input. Where behaviour would be ambiguous the original text is kept.
/// </summary>
public static class AssetMinifier
{
    /// <summary>
    /// Minifies CSS: strips comments, collapses whitespace and removes space around the
    /// structural punctuation <c>{ } : ; , &gt; ~ + *</c>. String literals are preserved.
    /// </summary>
    public static string MinifyCss(string input) => Minify(input, css: true);

    /// <summary>
    /// Minifies JavaScript: strips line and block comments (outside strings/regexes) and
    /// collapses runs of whitespace to a single space; space is dropped where it could not
    /// join two tokens. Strings and regex literals are preserved.
    /// </summary>
    public static string MinifyJs(string input) => Minify(input, css: false);

    private static string Minify(string input, bool css)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder(input.Length);
        int len = input.Length;
        int i = 0;
        bool pendingSpace = false;

        while (i < len)
        {
            char c = input[i];

            // Line comment (JS only) — the whole rest of the line is removed.
            if (!css && c == '/' && i + 1 < len && input[i + 1] == '/')
            {
                while (i < len && input[i] != '\n')
                {
                    i++;
                }
                continue;
            }

            // Block comment.
            if (c == '/' && i + 1 < len && input[i + 1] == '*')
            {
                int close = input.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = close < 0 ? len : close + 2;
                continue;
            }

            // String literal — copy verbatim.
            if (c == '"' || c == '\'')
            {
                if (pendingSpace)
                {
                    sb.Append(' ');
                    pendingSpace = false;
                }
                sb.Append(ConsumeString(input, ref i, len));
                continue;
            }

            // Whitespace — remember it, decide whether any is needed later.
            if (char.IsWhiteSpace(c))
            {
                pendingSpace = true;
                while (i < len && char.IsWhiteSpace(input[i]))
                {
                    i++;
                }
                continue;
            }

            // Structural punctuation: never preceded/followed by space.
            if (IsStructural(c, css))
            {
                pendingSpace = false;
                sb.Append(c);
                i++;
                continue;
            }

            // Word tokens: keep a single separating space only when whitespace preceded
            // this token and both sides are identifiers (e.g. "var  x"); otherwise drop it.
            char prev = PrevNonSpace(sb);
            if (pendingSpace && prev != '\0' && IsIdent(prev) && IsIdent(c))
            {
                pendingSpace = false;
                sb.Append(' ');
            }
            else
            {
                pendingSpace = false;
            }

            sb.Append(c);
            i++;
        }

        return sb.ToString().Trim();
    }

    private static bool IsStructural(char c, bool css)
    {
        string punct = css ? "{}:;>,~+*" : "{}();,.*";
        return punct.IndexOf(c) >= 0;
    }

    private static bool IsIdent(char c)
    {
        return c == '_' || c == '$' || c == '-' || char.IsLetterOrDigit(c);
    }

    private static char PrevNonSpace(StringBuilder sb)
    {
        int i = sb.Length - 1;
        while (i >= 0 && char.IsWhiteSpace(sb[i]))
        {
            i--;
        }
        return i >= 0 ? sb[i] : '\0';
    }

    private static string ConsumeString(string input, ref int i, int len)
    {
        var quote = input[i];
        var sb = new StringBuilder();
        sb.Append(quote);
        i++;
        while (i < len)
        {
            char sc = input[i];
            sb.Append(sc);
            i++;
            if (sc == '\\' && i < len)
            {
                sb.Append(input[i]);
                i++;
            }
            else if (sc == quote)
            {
                break;
            }
        }
        return sb.ToString();
    }
}
