using Fluid;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Service coordinating template contexts, partial lookups, and layout nesting renders.
/// </summary>
public class TemplateService(
    ITemplateRenderer renderer,
    ITemplateContextFactory contextFactory,
    ILogger logger,
    string layoutsDir
)
{
    private readonly ITemplateRenderer _renderer = renderer;
    private readonly ITemplateContextFactory _contextFactory = contextFactory;
    private readonly ILogger _logger = logger;
    private readonly string _layoutsDir = layoutsDir;

    /// <summary>
    /// Renders page body markup and resolves layout inheritance nesting.
    /// </summary>
    public async Task<string> RenderPageAsync(ContentDocument document, SiteContext siteContext)
    {
        // 1. Setup active page metadata in context
        siteContext.Page = document.Metadata.ToDictionary();
        siteContext.Page["content"] = document.Body;
        siteContext.Page["slug"] = document.Slug;

        // 2. Render page template body content
        // We evaluate variables/conditions/loops defined inside the page itself
        var pageContext = _contextFactory.CreateContext(siteContext, _layoutsDir);
        var renderedBody = await RenderContentWithContextAsync(document.Body, pageContext);

        // 3. Resolve layout inheritance (layouts may nest via their own frontmatter)
        if (!string.IsNullOrEmpty(document.Metadata.Layout))
        {
            return await RenderWithLayoutAsync(document.Metadata.Layout, renderedBody, siteContext);
        }

        return renderedBody;
    }

    /// <summary>
    /// Renders a layout, replacing its {{ content }} with the rendered body, then
    /// recursively resolves any parent layout declared in the layout frontmatter.
    /// </summary>
    private async Task<string> RenderWithLayoutAsync(
        string layoutName,
        string content,
        SiteContext siteContext
    )
    {
        var layoutFile = Path.Combine(_layoutsDir, $"{layoutName}.liquid");
        if (!File.Exists(layoutFile))
        {
            _logger.LogWarn($"Layout '{layoutName}' specified but was not found at: {layoutFile}");
            return content;
        }

        var layoutContent = await File.ReadAllTextAsync(layoutFile);

        // Parse optional frontmatter to detect parent layout declarations
        string body = layoutContent;
        string? parentLayout = null;
        if (layoutContent.StartsWith("---"))
        {
            var endIndex = layoutContent.IndexOf("---", 3);
            if (endIndex > 0)
            {
                var frontmatter = layoutContent.Substring(3, endIndex - 3).Trim();
                if (frontmatter.Contains("layout:", StringComparison.OrdinalIgnoreCase))
                {
                    var line = frontmatter
                        .Split('\n')
                        .FirstOrDefault(l =>
                            l.Trim().StartsWith("layout:", StringComparison.OrdinalIgnoreCase)
                        );
                    parentLayout = line?.Substring(line.IndexOf(':') + 1).Trim().Trim('\'', '"');
                }
                body = layoutContent.Substring(endIndex + 3).Trim();
            }
        }

        siteContext.Page["content"] = content;
        var layoutContext = _contextFactory.CreateContext(siteContext, _layoutsDir);
        var rendered = await RenderContentWithContextAsync(body, layoutContext);

        if (!string.IsNullOrEmpty(parentLayout))
        {
            return await RenderWithLayoutAsync(parentLayout, rendered, siteContext);
        }

        return rendered;
    }

    private async Task<string> RenderContentWithContextAsync(
        string content,
        TemplateContext context
    )
    {
        // FluidParser evaluates the parsed template relative to the configured context values
        var parser = new FluidParser();
        if (parser.TryParse(content, out var template, out var error))
        {
            return await template.RenderAsync(context);
        }
        throw new Exception($"Fluid template compilation error: {error}");
    }
}
