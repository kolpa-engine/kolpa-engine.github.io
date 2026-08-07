using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// Default class-based syntax highlighter. Emits semantic <c>hl-*</c> CSS classes
/// (never inline styles) so themes can be applied from a generated stylesheet.
/// Uses lightweight per-language tokenization tuned for static-site output.
/// </summary>
public class BuiltinSyntaxHighlighter(string classPrefix = "hl-") : ICodeHighlighter
{
    private readonly Dictionary<string, LanguageDef> _languages = BuildLanguages();
    private readonly string _classPrefix = classPrefix;

    public bool Supports(string language)
    {
        return ResolveLanguage(language) != null;
    }

    public string? Highlight(string code, string language)
    {
        var lang = ResolveLanguage(language);
        if (lang == null)
        {
            return null;
        }

        var html = new System.Text.StringBuilder();
        foreach (var (type, text) in Tokenize(code, lang))
        {
            var escaped = System.Net.WebUtility.HtmlEncode(text);
            if (type == "text")
            {
                html.Append(escaped);
            }
            else
            {
                html.Append($"<span class=\"{_classPrefix}{type}\">{escaped}</span>");
            }
        }

        return html.ToString();
    }

    private LanguageDef? ResolveLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var key = language.Trim().ToLowerInvariant();
        return _aliases.TryGetValue(key, out var canonical) ? _languages[canonical] : null;
    }

    private static IEnumerable<(string type, string text)> Tokenize(string code, LanguageDef lang)
    {
        int i = 0;
        int n = code.Length;

        while (i < n)
        {
            // Line comments
            bool matched = false;
            foreach (var marker in lang.LineComments)
            {
                if (Matches(code, i, marker))
                {
                    int end = code.IndexOf('\n', i);
                    end = end < 0 ? n : end;
                    yield return ("comment", code[i..end]);
                    i = end;
                    matched = true;
                    break;
                }
            }
            if (matched)
                continue;

            // Block comments
            if (lang.BlockCommentStart.Length > 0 && Matches(code, i, lang.BlockCommentStart))
            {
                int end = code.IndexOf(lang.BlockCommentEnd, i + lang.BlockCommentStart.Length);
                end = end < 0 ? n : end + lang.BlockCommentEnd.Length;
                yield return ("comment", code[i..end]);
                i = end;
                continue;
            }

            // Strings
            if (code[i] == '"' || code[i] == '\'' || (lang.BacktickStrings && code[i] == '`'))
            {
                var quote = code[i];
                int j = i + 1;
                while (j < n)
                {
                    if (code[j] == '\\' && j + 1 < n)
                    {
                        j += 2;
                        continue;
                    }
                    if (code[j] == quote)
                    {
                        j++;
                        break;
                    }
                    j++;
                }
                yield return ("string", code[i..j]);
                i = j;
                continue;
            }

            // Numbers
            if (char.IsDigit(code[i]) || (code[i] == '.' && i + 1 < n && char.IsDigit(code[i + 1])))
            {
                int j = i;
                while (
                    j < n
                    && (
                        char.IsLetterOrDigit(code[j])
                        || code[j] == '.'
                        || code[j] == '_'
                        || code[j] == '-'
                        || code[j] == '+'
                    )
                )
                {
                    j++;
                }
                yield return ("number", code[i..j]);
                i = j;
                continue;
            }

            // Words / identifiers
            if (
                char.IsLetter(code[i])
                || code[i] == '_'
                || (code[i] == '@' && i + 1 < n && char.IsLetter(code[i + 1]))
            )
            {
                int j = i;
                if (code[i] == '@')
                    j++;
                while (j < n && (char.IsLetterOrDigit(code[j]) || code[j] == '_'))
                {
                    j++;
                }
                var word = code[i..j];
                if (lang.Keywords.Contains(word))
                {
                    yield return ("keyword", word);
                }
                else if (lang.Types.Contains(word))
                {
                    yield return ("type", word);
                }
                else
                {
                    yield return ("text", word);
                }
                i = j;
                continue;
            }

            // Operators
            if ("=<>!&|+-*/%^~?:;,.(){}[]#".Contains(code[i]))
            {
                yield return ("operator", code[i].ToString());
                i++;
                continue;
            }

            yield return ("text", code[i].ToString());
            i++;
        }
    }

    private static bool Matches(string source, int index, string marker)
    {
        if (index + marker.Length > source.Length)
        {
            return false;
        }

        return string.CompareOrdinal(source, index, marker, 0, marker.Length) == 0;
    }

    private static readonly Dictionary<string, string> _aliases = new()
    {
        ["csharp"] = "csharp",
        ["c#"] = "csharp",
        ["cs"] = "csharp",
        ["cpp"] = "cpp",
        ["c++"] = "cpp",
        ["c"] = "cpp",
        ["cc"] = "cpp",
        ["hpp"] = "cpp",
        ["javascript"] = "javascript",
        ["js"] = "javascript",
        ["jsx"] = "javascript",
        ["typescript"] = "typescript",
        ["ts"] = "typescript",
        ["tsx"] = "typescript",
        ["json"] = "json",
        ["bash"] = "bash",
        ["sh"] = "bash",
        ["shell"] = "bash",
        ["zsh"] = "bash",
        ["console"] = "bash",
        ["python"] = "python",
        ["py"] = "python",
        ["css"] = "css",
        ["html"] = "markup",
        ["xml"] = "markup",
        ["svg"] = "markup",
        ["htm"] = "markup",
        ["glsl"] = "glsl",
        ["sql"] = "sql",
        ["java"] = "java",
        ["yaml"] = "yaml",
        ["yml"] = "yaml",
    };

    private static Dictionary<string, LanguageDef> BuildLanguages()
    {
        return new Dictionary<string, LanguageDef>(StringComparer.OrdinalIgnoreCase)
        {
            ["csharp"] = new LanguageDef(
                ["//"],
                "/*",
                "*/",
                "\"'",
                false,
                Keywords(
                    "public private protected internal static readonly const class struct interface enum record namespace using var new return if else for foreach while do switch case break continue throw try catch finally this base null true false get set async await override virtual sealed partial void int string float double decimal bool char long short byte object dynamic out ref in params is as typeof nameof default where yield lock unsafe goto extern event delegate operator implicit explicit"
                ),
                Types(
                    "string int long float double decimal bool char byte short object dynamic void DateTime TimeSpan Guid List Dictionary Task Task<> IEnumerable string[] object[] var"
                )
            ),

            ["cpp"] = new LanguageDef(
                ["//"],
                "/*",
                "*/",
                "\"'",
                false,
                Keywords(
                    "public private protected static const class struct enum namespace using return if else for while do switch case break continue throw try catch this null true false new delete void int float double bool char long short unsigned signed constexpr auto inline virtual override const_cast static_cast dynamic_cast reinterpret_cast template typename typename operator sizeof include define ifdef ifndef endif pragma noexcept override"
                ),
                Types(
                    "int float double bool char long short void string vector map unordered_map set unique_ptr shared_ptr auto size_t int32_t uint32_t"
                )
            ),

            ["javascript"] = new LanguageDef(
                ["//"],
                "/*",
                "*/",
                "\"'`",
                true,
                Keywords(
                    "const let var function return if else for while do class new this import export from default async await null undefined true false throw try catch finally typeof instanceof switch case break continue delete in of static extends super yield void with get set"
                ),
                Types("Object Array String Number Boolean Function Promise Map Set Symbol BigInt")
            ),

            ["typescript"] = new LanguageDef(
                ["//"],
                "/*",
                "*/",
                "\"'`",
                true,
                Keywords(
                    "const let var function return if else for while do class new this import export from default async await null undefined true false throw try catch finally typeof instanceof switch case break continue delete in of static extends super yield void with interface type enum namespace declare readonly implements private protected public get set"
                ),
                Types(
                    "Object Array String Number Boolean Function Promise Map Set Symbol BigInt any unknown never void string number boolean"
                )
            ),

            ["json"] = new LanguageDef(
                [],
                string.Empty,
                string.Empty,
                "\"",
                false,
                Keywords("true false null"),
                Types()
            ),

            ["bash"] = new LanguageDef(
                ["#"],
                string.Empty,
                string.Empty,
                "\"'`",
                true,
                Keywords(
                    "if then else elif fi for in do done while case esac function echo export local return exit read set shift source sudo cd ls grep sed awk curl wget mkdir rm cp mv touch cat printf test declare let select until true false break continue"
                ),
                Types()
            ),

            ["python"] = new LanguageDef(
                ["#"],
                string.Empty,
                string.Empty,
                "\"'`",
                true,
                Keywords(
                    "def return if elif else for while import from as class try except finally raise with lambda pass break continue global nonlocal yield in not and or is None True False del assert async await"
                ),
                Types("str int float bool list dict tuple set bytes object NoneType")
            ),

            ["css"] = new LanguageDef(
                [],
                "/*",
                "*/",
                "\"'",
                false,
                Keywords(
                    "color background margin padding border display position top right bottom left width height font size weight flex grid align justify overflow opacity transform transition animation media hover active focus"
                ),
                Types()
            ),

            ["markup"] = new LanguageDef(
                [],
                "<!--",
                "-->",
                "\"'",
                false,
                Keywords(
                    "div span a p h1 h2 h3 h4 h5 h6 ul ol li img input button form table tr td th thead tbody head body html meta link script style section article header footer nav main aside class id href src alt title rel target xmlns version lang type name value checked placeholder required data "
                ),
                Types()
            ),

            ["glsl"] = new LanguageDef(
                ["//"],
                "/*",
                "*/",
                "\"'",
                false,
                Keywords(
                    "uniform in out inout const return if else for while true false struct void attribute varying discard break continue"
                ),
                Types(
                    "vec2 vec3 vec4 ivec2 ivec3 ivec4 mat2 mat3 mat4 sampler2D samplerCube sampler2DArray float int bool uint double"
                ),
                [
                    "texture",
                    "dot",
                    "cross",
                    "normalize",
                    "clamp",
                    "mix",
                    "pow",
                    "smoothstep",
                    "step",
                    "length",
                    "distance",
                    "max",
                    "min",
                    "abs",
                    "floor",
                    "ceil",
                    "fract",
                    "mod",
                    "sin",
                    "cos",
                    "tan",
                    "exp",
                    "log",
                    "inversesqrt",
                ]
            ),

            ["sql"] = new LanguageDef(
                ["--"],
                "/*",
                "*/",
                "\"'",
                false,
                Keywords(
                    "SELECT FROM WHERE INSERT UPDATE DELETE CREATE TABLE DROP ALTER JOIN INNER LEFT RIGHT FULL OUTER ON GROUP BY ORDER HAVING LIMIT OFFSET AS AND OR NOT IN VALUES INTO SET PRIMARY KEY FOREIGN REFERENCES DEFAULT NULL DISTINCT UNION ALL CASE WHEN THEN ELSE END BETWEEN LIKE IS EXISTS DESC ASC"
                ),
                Types(
                    "INTEGER INT BIGINT VARCHAR TEXT CHAR BOOLEAN BOOL DATE DATETIME TIMESTAMP FLOAT DOUBLE DECIMAL NUMERIC BLOB UUID"
                )
            ),

            ["java"] = new LanguageDef(
                ["//"],
                "/*",
                "*/",
                "\"'",
                false,
                Keywords(
                    "public private protected static final class interface enum extends implements new return if else for while do switch case break continue throw throws try catch finally this super null true false void import package abstract synchronized volatile transient native strictfp default"
                ),
                Types(
                    "int long float double boolean char byte short String Object Integer Long Float Double Boolean Character List ArrayList Map HashMap Set Collection Optional void"
                )
            ),

            ["yaml"] = new LanguageDef(
                ["#"],
                string.Empty,
                string.Empty,
                "\"'",
                false,
                Keywords("true false null yes no on off"),
                Types()
            ),
        };
    }

    private static HashSet<string> Keywords(string spaceSeparated)
    {
        return new HashSet<string>(
            spaceSeparated.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ),
            StringComparer.Ordinal
        );
    }

    private static HashSet<string> Types(string spaceSeparated = "")
    {
        return new HashSet<string>(
            spaceSeparated.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ),
            StringComparer.Ordinal
        );
    }

    private sealed record LanguageDef(
        string[] LineComments,
        string BlockCommentStart,
        string BlockCommentEnd,
        string QuoteChars,
        bool BacktickStrings,
        HashSet<string> Keywords,
        HashSet<string> Types,
        string[]? BuiltinFunctions = null
    );
}
