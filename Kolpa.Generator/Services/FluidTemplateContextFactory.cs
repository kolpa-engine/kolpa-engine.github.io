using System.Globalization;
using Fluid;
using Fluid.Values;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Microsoft.Extensions.FileProviders;

namespace Kolpa.Generator.Services;

/// <summary>
/// Implements Fluid TemplateContext creation and type registration mapping.
/// </summary>
public class FluidTemplateContextFactory : ITemplateContextFactory
{
    public TemplateContext CreateContext(SiteContext siteContext, string layoutsDir)
    {
        var templateContext = new TemplateContext();

        //  register Member Access Strategies for models
        templateContext.Options.MemberAccessStrategy.Register<SiteContext>();
        templateContext.Options.MemberAccessStrategy.Register<ContentDocument>();
        templateContext.Options.MemberAccessStrategy.Register<ContentMetadata>();
        templateContext.Options.MemberAccessStrategy.Register<Dictionary<string, object>>();
        templateContext.Options.MemberAccessStrategy.Register<List<Dictionary<string, object>>>();
        templateContext.Options.MemberAccessStrategy.Register<List<ContentDocument>>();
        templateContext.Options.MemberAccessStrategy.IgnoreCasing = true;

        //  set root values
        templateContext.SetValue("site", siteContext.Site);
        templateContext.SetValue("data", siteContext.Data);
        templateContext.SetValue("collections", siteContext.Collections);
        templateContext.SetValue("tagcloud", siteContext.Tags);
        templateContext.SetValue("page", siteContext.Page);

        // Render page metadata fields directly at the root (e.g. {{ title }} or {{ layout }})
        foreach (var kvp in siteContext.Page)
        {
            templateContext.SetValue(kvp.Key, kvp.Value);
        }

        // 3. Setup layouts and includes file resolution
        if (Directory.Exists(layoutsDir))
        {
            templateContext.Options.FileProvider = new PhysicalFileProvider(
                Path.GetFullPath(layoutsDir)
            );
        }

        // 4. Custom Filters (AAA-standard template extension point)
        templateContext.Options.Filters.AddFilter(
            "date_format",
            (input, arguments, context) =>
            {
                var obj = input.ToObjectValue();
                DateTime dt;
                if (obj is DateTime dateValue)
                {
                    dt = dateValue;
                }
                else if (obj is DateTimeOffset dateOffset)
                {
                    dt = dateOffset.DateTime;
                }
                else if (obj != null && DateTime.TryParse(obj.ToString(), out var parsed))
                {
                    dt = parsed;
                }
                else
                {
                    return input;
                }

                var format = arguments.At(0).ToStringValue() ?? "yyyy-MM-dd";
                return new StringValue(dt.ToString(format, CultureInfo.InvariantCulture));
            }
        );

        templateContext.Options.Filters.AddFilter(
            "limit",
            (input, arguments, context) =>
            {
                if (input.ToObjectValue() is IEnumerable<object> list)
                {
                    var countVal = arguments.At(0).ToNumberValue();
                    int count = Convert.ToInt32(countVal);
                    var limited = new List<object>();
                    int current = 0;
                    foreach (var item in list)
                    {
                        if (current >= count)
                            break;
                        limited.Add(item);
                        current++;
                    }
                    return new ArrayValue(
                        limited.Select(item => FluidValue.Create(item, context.Options)).ToArray()
                    );
                }
                return input;
            }
        );

        // Slugify: turn any string into a URL-safe lowercase kebab slug.
        templateContext.Options.Filters.AddFilter(
            "slugify",
            (input, arguments, context) =>
            {
                var raw = input.ToStringValue() ?? "";
                var slug = System.Text.RegularExpressions.Regex.Replace(
                    raw.Trim().ToLowerInvariant(),
                    "[^a-z0-9]+",
                    "-"
                );
                slug = slug.Trim('-');
                return new StringValue(slug);
            }
        );

        // read_time: estimate minutes to read from an HTML content string (~200 wpm).
        templateContext.Options.Filters.AddFilter(
            "read_time",
            (input, arguments, context) =>
            {
                var html = input.ToStringValue() ?? "";
                var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
                var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
                int minutes = Math.Max(1, (int)Math.Ceiling(words / 200.0));
                return NumberValue.Create(minutes);
            }
        );

        return templateContext;
    }
}
