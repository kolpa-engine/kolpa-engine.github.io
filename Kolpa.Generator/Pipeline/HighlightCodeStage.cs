using System.Text.RegularExpressions;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Highlights fenced code blocks inside rendered HTML. It locates
/// <c>&lt;pre&gt;&lt;code class="language-X"&gt;</c> blocks, runs their content through the
/// configured <c>ICodeHighlighter</c>, and marks them with the <c>highlighted</c> class
/// plus the active theme class (never inline styles).
/// </summary>
public partial class HighlightCodeStage(
    ICodeHighlighter highlighter,
    GeneratorConfig config,
    ILogger logger
) : IBuildStage
{
    private readonly ICodeHighlighter _highlighter = highlighter;
    private readonly HighlightingSettings _settings = config.Markdown.Highlighting;
    private readonly ILogger _logger = logger;

    public string Name => "Highlight Code";

    public Task ExecuteAsync(BuildContext context)
    {
        if (!_settings.Enabled)
        {
            _logger.LogVerbose("[Highlight] Disabled by configuration.");
            return Task.CompletedTask;
        }

        var themeClass = $"{_settings.CssPrefix}theme-{Sanitize(_settings.Theme)}";
        var regex = CodeBlockRegex();

        int blocks = 0;
        int highlighted = 0;

        foreach (var doc in context.Documents)
        {
            if (string.IsNullOrEmpty(doc.Body))
            {
                continue;
            }

            doc.Body = regex.Replace(
                doc.Body,
                m =>
                {
                    blocks++;
                    var language = m.Groups["lang"].Value;
                    var inner = m.Groups["content"].Value;

                    var decoded = System.Net.WebUtility.HtmlDecode(inner);
                    var result = _highlighter.Highlight(decoded, language);
                    var newInner = result ?? inner;
                    if (result != null)
                    {
                        highlighted++;
                    }

                    var langClass = System.Net.WebUtility.HtmlEncode(language);
                    return $"<pre class=\"{themeClass}\"><code class=\"language-{langClass} highlighted\">{newInner}</code></pre>";
                }
            );
        }

        _logger.LogInfo(
            $"[Highlight] {highlighted} code block(s) highlighted across {blocks} block(s)."
        );

        context.AddDiagnostic(
            DiagnosticSeverity.Info,
            $"Highlighted {highlighted} of {blocks} code block(s).",
            Name
        );

        return Task.CompletedTask;
    }

    [GeneratedRegex(
        "<pre>\\s*<code class=\"language-(?<lang>[^\"\\s]+)\">(?<content>.*?)</code></pre>",
        RegexOptions.Singleline
    )]
    private static partial Regex CodeBlockRegex();

    private static string Sanitize(string name)
    {
        return new string([.. name.Where(char.IsLetterOrDigit)]);
    }
}
