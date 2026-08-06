using Fluid;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Service coordinating template contexts, partial lookups, and layout nesting renders.
/// </summary>
public class TemplateService
{
    private readonly ITemplateRenderer _renderer;
    private readonly ITemplateContextFactory _contextFactory;
    private readonly ILogger _logger;
    private readonly string _layoutsDir;

    public TemplateService(
        ITemplateRenderer renderer,
        ITemplateContextFactory contextFactory,
        ILogger logger,
        string layoutsDir)
    {
        _renderer = renderer;
        _contextFactory = contextFactory;
        _logger = logger;
        _layoutsDir = layoutsDir;
    }

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

        // 3. Resolve layout inheritance
        if (!string.IsNullOrEmpty(document.Metadata.Layout))
        {
            var layoutFile = Path.Combine(_layoutsDir, $"{document.Metadata.Layout}.liquid");
            if (File.Exists(layoutFile))
            {
                var layoutContent = await File.ReadAllTextAsync(layoutFile);

                // Update context page object content with rendered body
                siteContext.Page["content"] = renderedBody;

                var layoutContext = _contextFactory.CreateContext(siteContext, _layoutsDir);
                return await RenderContentWithContextAsync(layoutContent, layoutContext);
            }
            else
            {
                _logger.LogWarn($"Layout '{document.Metadata.Layout}' specified in document '{document.Id}' but was not found at: {layoutFile}");
            }
        }

        return renderedBody;
    }

    private async Task<string> RenderContentWithContextAsync(string content, TemplateContext context)
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
