using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace Kolpa.Generator.Services;

/// <summary>
/// Renders Markdown to HTML using a Markdig pipeline assembled from configuration.
/// Syntax highlighting is intentionally left to a later, separate stage.
/// </summary>
public class MarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownRenderer(GeneratorConfig config)
    {
        var extensions = config.Markdown.Extensions;
        var builder = new MarkdownPipelineBuilder();

        if (extensions.Advanced)
        {
            builder.UseAdvancedExtensions();
        }

        if (extensions.Tables)
        {
            builder.UsePipeTables();
        }

        if (extensions.TaskLists)
        {
            builder.UseTaskLists();
        }

        if (extensions.Footnotes)
        {
            builder.UseFootnotes();
        }

        if (extensions.AutoIdentifiers)
        {
            builder.UseAutoIdentifiers();
        }

        if (extensions.Strikethrough)
        {
            builder.UseEmphasisExtras(EmphasisExtraOptions.Strikethrough);
        }

        if (extensions.AutoLinks)
        {
            builder.UseAutoLinks();
        }

        if (extensions.DefinitionLists)
        {
            builder.UseDefinitionLists();
        }

        if (extensions.EmojiSmiles)
        {
            builder.UseEmojiAndSmiley();
        }

        if (extensions.Mathematics)
        {
            builder.UseMathematics();
        }

        _pipeline = builder.Build();
    }

    public string Render(string markdown)
    {
        return Markdown.ToHtml(markdown ?? string.Empty, _pipeline);
    }
}
