using System.Text;
using Kolpa.Generator.Config;

namespace Kolpa.Generator.Services;

/// <summary>
/// Resolves named highlighting themes (light/dark/custom) into token color maps and
/// generates a reusable, themeable stylesheet. Colors are applied via CSS classes only,
/// never inline styles, so websites can override or extend the generated asset.
/// </summary>
public static class HighlightTheme
{
    private static readonly Dictionary<string, Dictionary<string, string>> BuiltIn = new()
    {
        ["light"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["keyword"] = "#d73a49",
            ["type"] = "#6f42c1",
            ["string"] = "#032f62",
            ["comment"] = "#6a737d",
            ["number"] = "#005cc5",
            ["operator"] = "#24292e",
            ["function"] = "#6f42c1",
        },
        ["dark"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["keyword"] = "#ff7b72",
            ["type"] = "#d2a8ff",
            ["string"] = "#a5d6ff",
            ["comment"] = "#8b949e",
            ["number"] = "#79c0ff",
            ["operator"] = "#c9d1d9",
            ["function"] = "#d2a8ff",
        },
    };

    /// <summary>
    /// Resolves the effective theme map (built-in or custom from configuration).
    /// </summary>
    public static Dictionary<string, string> ResolveTheme(HighlightingSettings settings)
    {
        var themeName = string.IsNullOrWhiteSpace(settings.Theme) ? "dark" : settings.Theme;
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (BuiltIn.TryGetValue(themeName, out var builtIn))
        {
            foreach (var kvp in builtIn)
            {
                tokens[kvp.Key] = kvp.Value;
            }
        }

        foreach (var kvp in settings.CustomTheme)
        {
            tokens[kvp.Key.Trim().TrimStart(settings.CssPrefix.ToCharArray())] = kvp.Value;
        }

        if (tokens.Count == 0)
        {
            foreach (var kvp in BuiltIn["dark"])
            {
                tokens[kvp.Key] = kvp.Value;
            }
        }

        return tokens;
    }

    /// <summary>
    /// Generates a reusable stylesheet mapping token classes to theme colors.
    /// Rules are scoped to a theme class so multiple themes can coexist.
    /// </summary>
    public static string GenerateCss(
        HighlightingSettings settings,
        Dictionary<string, string> themeMap
    )
    {
        var themeName = string.IsNullOrWhiteSpace(settings.Theme) ? "dark" : settings.Theme;
        var prefix = settings.CssPrefix;
        var sb = new StringBuilder();

        sb.AppendLine($"/* Auto-generated syntax highlighting theme: {themeName} */");
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}keyword {{ color: {Color(themeMap, "keyword")}; }}"
        );
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}type {{ color: {Color(themeMap, "type")}; }}"
        );
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}string {{ color: {Color(themeMap, "string")}; }}"
        );
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}comment {{ color: {Color(themeMap, "comment")}; font-style: italic; }}"
        );
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}number {{ color: {Color(themeMap, "number")}; }}"
        );
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}operator {{ color: {Color(themeMap, "operator")}; }}"
        );
        sb.AppendLine(
            $".{prefix}theme-{Sanitize(themeName)} .{prefix}function {{ color: {Color(themeMap, "function")}; }}"
        );

        return sb.ToString();
    }

    private static string Color(Dictionary<string, string> theme, string token)
    {
        return theme.TryGetValue(token, out var color) ? color : "#9da5b4";
    }

    private static string Sanitize(string name)
    {
        return new string(name.Where(char.IsLetterOrDigit).ToArray());
    }
}
