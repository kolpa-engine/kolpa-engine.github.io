using System.Collections.Concurrent;
using Fluid;
using Microsoft.Extensions.FileProviders;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Implements template rendering using the Fluid Liquid template engine.
/// </summary>
public class FluidTemplateRenderer : ITemplateRenderer
{
    private readonly FluidParser _parser = new();
    private readonly ConcurrentDictionary<string, IFluidTemplate> _templateCache = new();
    private readonly string _layoutsDir;

    public FluidTemplateRenderer(string layoutsDir)
    {
        _layoutsDir = layoutsDir;
    }

    /// <summary>
    /// Renders a template content string with the provided context.
    /// </summary>
    public async Task<string> RenderAsync(string templateContent, object context)
    {
        var template = _templateCache.GetOrAdd(templateContent, content =>
        {
            if (!_parser.TryParse(content, out var parsedTemplate, out var error))
            {
                throw new Exception($"Fluid Template parsing error: {error}");
            }
            return parsedTemplate;
        });

        var templateContext = new TemplateContext();

        // Setup Member Access Strategy for dictionaries and properties
        templateContext.Options.MemberAccessStrategy.Register<SiteContext>();
        templateContext.Options.MemberAccessStrategy.Register<Dictionary<string, object>>();
        templateContext.Options.MemberAccessStrategy.Register<List<Dictionary<string, object>>>();

        // Allow any dictionary access by default
        templateContext.Options.MemberAccessStrategy.IgnoreCasing = true;

        if (context is SiteContext siteCtx)
        {
            templateContext.SetValue("site", siteCtx.Site);
            templateContext.SetValue("data", siteCtx.Data);
            templateContext.SetValue("collections", siteCtx.Collections);
            templateContext.SetValue("page", siteCtx.Page);

            // Allow root access to page properties directly (e.g. {{ title }})
            foreach (var kvp in siteCtx.Page)
            {
                templateContext.SetValue(kvp.Key, kvp.Value);
            }
        }
        else if (context is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                templateContext.SetValue(kvp.Key, kvp.Value);
            }
        }

        // Configure Layouts / Includes file resolution
        if (Directory.Exists(_layoutsDir))
        {
            templateContext.Options.FileProvider = new PhysicalFileProvider(Path.GetFullPath(_layoutsDir));
        }

        return await template.RenderAsync(templateContext);
    }
}
