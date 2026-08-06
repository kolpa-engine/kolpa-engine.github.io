using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Builds a <see cref="SiteContext"/> from a <see cref="BuildContext"/> using identical
/// rules in every stage that needs template data, so the two can never drift apart.
/// </summary>
public static class SiteContextFactory
{
    public static SiteContext Create(BuildContext context)
    {
        var siteContext = new SiteContext();
        siteContext.Site["title"] = context.Config.Site.Title;
        siteContext.Site["description"] = context.Config.Site.Description;
        siteContext.Site["url"] = context.Config.Site.Url;
        siteContext.Site["showWarningBanner"] = context.Config.Site.ShowWarningBanner;
        siteContext.Site["warningBannerText"] = context.Config.Site.WarningBannerText;

        siteContext.Urls = [.. context
            .Routes.Select(r => r.Url)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(u => u, StringComparer.OrdinalIgnoreCase)];

        foreach (var dataKvp in context.DataRegistry)
        {
            siteContext.Data[dataKvp.Key] = dataKvp.Value;
        }

        if (
            context.Metadata.TryGetValue("images", out var imagesObj)
            && imagesObj is Dictionary<string, object> images
        )
        {
            foreach (var imageKvp in images)
            {
                siteContext.Images[imageKvp.Key] = imageKvp.Value;
            }
        }

        foreach (var collKvp in context.Collections)
        {
            var rawList = collKvp
                .Value.OrderByDescending(doc => doc.Metadata.Date ?? DateTime.MinValue)
                .Select(doc =>
                {
                    var dict = doc.Metadata.ToDictionary();
                    dict["content"] = doc.Body;
                    dict["slug"] = doc.Slug;
                    dict["url"] = doc.OutputUrl;
                    return dict;
                })
                .ToList();

            siteContext.Collections[collKvp.Key] = rawList;
        }

        return siteContext;
    }
}
