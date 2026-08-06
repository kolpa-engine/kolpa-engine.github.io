using Fluid;
using Fluid.Values;
using Microsoft.Extensions.FileProviders;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Implements Fluid TemplateContext creation and type registration mapping.
/// </summary>
public class FluidTemplateContextFactory : ITemplateContextFactory
{
    public TemplateContext CreateContext(SiteContext siteContext, string layoutsDir)
    {
        var templateContext = new TemplateContext();

        // 1. Register Member Access Strategies for models
        templateContext.Options.MemberAccessStrategy.Register<SiteContext>();
        templateContext.Options.MemberAccessStrategy.Register<ContentDocument>();
        templateContext.Options.MemberAccessStrategy.Register<ContentMetadata>();
        templateContext.Options.MemberAccessStrategy.Register<Dictionary<string, object>>();
        templateContext.Options.MemberAccessStrategy.Register<List<Dictionary<string, object>>>();
        templateContext.Options.MemberAccessStrategy.Register<List<ContentDocument>>();
        templateContext.Options.MemberAccessStrategy.IgnoreCasing = true;

        // 2. Set root values
        templateContext.SetValue("site", siteContext.Site);
        templateContext.SetValue("data", siteContext.Data);
        templateContext.SetValue("collections", siteContext.Collections);
        templateContext.SetValue("page", siteContext.Page);

        // Render page metadata fields directly at the root (e.g. {{ title }} or {{ layout }})
        foreach (var kvp in siteContext.Page)
        {
            templateContext.SetValue(kvp.Key, kvp.Value);
        }

        // 3. Setup layouts and includes file resolution
        if (Directory.Exists(layoutsDir))
        {
            templateContext.Options.FileProvider = new PhysicalFileProvider(Path.GetFullPath(layoutsDir));
        }

        // 4. Custom Filters (AAA-standard template extension point)
        templateContext.Options.Filters.AddFilter("date_format", (input, arguments, context) =>
        {
            if (input.ToObjectValue() is DateTime dt)
            {
                var format = arguments.At(0).ToStringValue() ?? "yyyy-MM-dd";
                return new StringValue(dt.ToString(format));
            }
            return input;
        });

        templateContext.Options.Filters.AddFilter("limit", (input, arguments, context) =>
        {
            if (input.ToObjectValue() is IEnumerable<object> list)
            {
                var countVal = arguments.At(0).ToNumberValue();
                int count = Convert.ToInt32(countVal);
                var limited = new List<object>();
                int current = 0;
                foreach (var item in list)
                {
                    if (current >= count) break;
                    limited.Add(item);
                    current++;
                }
                return new ArrayValue(limited.Select(item => FluidValue.Create(item, context.Options)).ToArray());
            }
            return input;
        });

        return templateContext;
    }
}
