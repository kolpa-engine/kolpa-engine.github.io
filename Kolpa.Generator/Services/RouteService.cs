using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Implements route generation and output file paths mappings.
/// </summary>
public class RouteService : IRouteGenerator
{
    public string GenerateCleanUrl(ContentDocument document, string pattern)
    {
        string slug = document.Slug;

        // If it's a page named "index", it maps to root "/"
        if (slug.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        // If index is inside a directory (e.g. "blog/index" -> "/blog/")
        if (slug.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat("/", slug.AsSpan(0, slug.Length - 6), "/");
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return $"/{slug}/";
        }

        // Handle path patterns (e.g. "/blog/{slug}/")
        // Remove collection prefix from the slug if it already exists in the pattern prefix to avoid duplication
        // E.g. pattern="/blog/{slug}/", slug="blog/first-post" -> we extract "first-post" to replace {slug}
        string processedSlug = slug;
        if (pattern.StartsWith("/") && pattern.Contains("/{slug}"))
        {
            var prefix = pattern.Substring(0, pattern.IndexOf("/{slug}"));
            var cleanPrefix = prefix.TrimStart('/');
            if (!string.IsNullOrEmpty(cleanPrefix) && slug.StartsWith(cleanPrefix + "/"))
            {
                processedSlug = slug.Substring(cleanPrefix.Length + 1);
            }
        }

        var resolved = pattern.Replace("{slug}", processedSlug);

        // Normalize slashes
        if (!resolved.StartsWith("/")) resolved = "/" + resolved;
        return resolved;
    }

    public string GetPhysicalOutputPath(string outputDir, string cleanUrl)
    {
        // Normalize clean URL
        var normalized = cleanUrl.Trim('/');

        if (string.IsNullOrEmpty(normalized))
        {
            return Path.Combine(outputDir, "index.html");
        }

        // Map "/blog/my-post/" -> "dist/blog/my-post/index.html"
        return Path.Combine(outputDir, normalized, "index.html");
    }
}
