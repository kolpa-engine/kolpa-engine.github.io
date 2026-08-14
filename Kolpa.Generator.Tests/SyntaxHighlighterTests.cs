using Kolpa.Generator.Config;
using Kolpa.Generator.Models;
using Kolpa.Generator.Pipeline;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Tests;

public class SyntaxHighlighterTests
{
    [Theory]
    [InlineData("csharp")]
    [InlineData("cpp")]
    [InlineData("javascript")]
    [InlineData("json")]
    [InlineData("bash")]
    public void Supports_Requested_Languages(string language)
    {
        Assert.True(new BuiltinSyntaxHighlighter().Supports(language));
    }

    [Theory]
    [InlineData("c#")]
    [InlineData("c++")]
    [InlineData("js")]
    [InlineData("sh")]
    public void Supports_Aliases(string alias)
    {
        Assert.True(new BuiltinSyntaxHighlighter().Supports(alias));
    }

    [Fact]
    public void Highlights_Csharp_Keywords_With_Classes_Not_Styles()
    {
        var html = new BuiltinSyntaxHighlighter("hl-").Highlight("public class Player", "csharp");

        Assert.NotNull(html);
        Assert.Contains("class=\"hl-keyword\"", html);
        Assert.DoesNotContain("style=", html);
    }

    [Fact]
    public void Returns_Null_For_Unknown_Language()
    {
        Assert.Null(new BuiltinSyntaxHighlighter().Highlight("var x;", "notalanguage"));
    }

    [Fact]
    public void Passthrough_Encodes_Code()
    {
        var html = new PassthroughCodeHighlighter().Highlight("a < b && c > 0", "csharp");

        Assert.NotNull(html);
        Assert.Contains("&lt;", html);
        Assert.DoesNotContain("<span", html);
    }

    [Fact]
    public async Task HighlightStage_Adds_Theme_And_Highlighted_Classes()
    {
        var config = new GeneratorConfig
        {
            Markdown = new MarkdownSettings
            {
                Highlighting = new HighlightingSettings
                {
                    Enabled = true,
                    Theme = "dark",
                    CssPrefix = "hl-",
                },
            },
        };

        var stage = new HighlightCodeStage(
            new BuiltinSyntaxHighlighter("hl-"),
            config,
            new NullLogger()
        );
        var context = new BuildContext(TestHelpers.TempDir());
        context.Documents.Add(
            new ContentDocument
            {
                Format = "markdown",
                Body = "<pre><code class=\"language-csharp\">public class Player</code></pre>",
            }
        );

        await stage.ExecuteAsync(context);

        Assert.Contains("class=\"hl-theme-dark\"", context.Documents[0].Body);
        Assert.Contains("class=\"language-csharp highlighted\"", context.Documents[0].Body);
        Assert.Contains("hl-keyword", context.Documents[0].Body);
    }

    [Fact]
    public void Generated_Theme_Css_Is_Reusable_And_Scoped()
    {
        var settings = new HighlightingSettings { Theme = "dark", CssPrefix = "hl-" };
        var css = HighlightTheme.GenerateCss(settings, HighlightTheme.ResolveTheme(settings));

        Assert.Contains(".hl-theme-dark .hl-keyword", css);
        Assert.Contains(".hl-theme-dark .hl-comment", css);
    }
}
