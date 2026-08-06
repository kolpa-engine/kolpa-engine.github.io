using Kolpa.Generator.Config;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Tests;

public class MarkdownRendererTests
{
    private static MarkdownRenderer Create()
    {
        return new MarkdownRenderer(new GeneratorConfig());
    }

    [Fact]
    public void Renders_Headings_And_Paragraphs()
    {
        var html = Create().Render("# Hello\n\nSome *emphasis* text.");

        Assert.Contains("<h1", html);
        Assert.Contains("Hello", html);
        Assert.Contains("<em>emphasis</em>", html);
    }

    [Fact]
    public void Renders_Tables()
    {
        var html = Create().Render("| A | B |\n|---|---|\n| 1 | 2 |");

        Assert.Contains("<table>", html);
        Assert.Contains("<td>", html);
    }

    [Fact]
    public void Renders_TaskLists()
    {
        var html = Create().Render("- [x] done\n- [ ] todo");

        Assert.Contains("checkbox", html);
        Assert.Contains("checked", html);
    }

    [Fact]
    public void Renders_Footnotes()
    {
        var html = Create().Render("Reference[^1]\n\n[^1]: The footnote text.");

        Assert.Contains("class=\"footnotes\"", html);
        Assert.Contains("fn:1", html);
    }

    [Fact]
    public void Preserves_Fenced_Code_Language_Class()
    {
        var html = Create().Render("```csharp\nvar x = 1;\n```");

        Assert.Contains("language-csharp", html);
        Assert.Contains("<pre><code", html);
    }
}
