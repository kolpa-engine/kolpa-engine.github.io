using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Validates a <see cref="GeneratorConfig"/> (and the enclosing project layout) and
/// produces a deterministic, code-addressable list of <see cref="ConfigIssue"/> findings.
/// Used by the <c>doctor</c> command and surfaced during builds so config problems are
/// never silent.
/// </summary>
public class ConfigValidator
{
    /// <summary>Well-known allowed highlighting providers.</summary>
    private static readonly HashSet<string> AllowedHighlighters = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "builtin",
        "passthrough",
    };

    /// <summary>Well-known allowed image processors.</summary>
    private static readonly HashSet<string> AllowedImageProcessors = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "imagesharp",
        "passthrough",
    };

    /// <summary>Built-in highlighting themes (extendable via <c>customTheme</c>).</summary>
    private static readonly HashSet<string> BuiltInThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "light",
        "dark",
    };

    private readonly IFileSystem _fileSystem;
    private readonly string _projectRoot;

    public ConfigValidator(IFileSystem fileSystem, string projectRoot)
    {
        _fileSystem = fileSystem;
        _projectRoot = Path.GetFullPath(projectRoot);
    }

    /// <summary>
    /// Runs all validation rules against the given config. Folder checks are resolved
    /// relative to the configured project root.
    /// </summary>
    public IReadOnlyList<ConfigIssue> Validate(GeneratorConfig config)
    {
        List<ConfigIssue> issues = [];
        if (config == null)
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Error,
                    "CFG000",
                    "Configuration object was null."
                )
            );
            return issues;
        }

        ValidateSite(config, issues);
        ValidatePaths(config, issues);
        ValidateRenderer(config, issues);
        ValidateMarkdown(config, issues);
        ValidateImages(config, issues);
        ValidateCache(config, issues);
        ValidateRedirects(config, issues);
        ValidateCollections(config, issues);
        ValidateRss(config, issues);

        return issues;
    }

    private void ValidateSite(GeneratorConfig config, List<ConfigIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(config.Site.Title))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Info,
                    "SITE001",
                    "'site.title' is empty. Consider providing a title for generated pages/feeds."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(config.Site.Url))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "SITE002",
                    "'site.url' is empty. RSS feed and sitemap generation require an absolute URL."
                )
            );
        }

        if (
            config.Site.ShowWarningBanner
            && string.IsNullOrWhiteSpace(config.Site.WarningBannerText)
        )
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "SITE003",
                    "'site.showWarningBanner' is enabled but 'site.warningBannerText' is empty."
                )
            );
        }
    }

    private void ValidatePaths(GeneratorConfig config, List<ConfigIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(config.Paths.Output))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Error,
                    "PATH001",
                    "'paths.output' is empty. A build output directory is required."
                )
            );
        }

        CheckSourceFolder(config.Paths.Pages, "pages", "PATH002", issues);
        CheckSourceFolder(config.Paths.Layouts, "layouts", "PATH003", issues);
        CheckSourceFolder(config.Paths.Content, "content", "PATH004", issues);
        CheckSourceFolder(config.Paths.Assets, "assets", "PATH005", issues);
    }

    private void CheckSourceFolder(
        string configured,
        string displayName,
        string code,
        List<ConfigIssue> issues
    )
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    code,
                    $"'paths.{displayName}' is not set."
                )
            );
            return;
        }

        var full = Path.GetFullPath(Path.Combine(_projectRoot, configured));
        if (!_fileSystem.DirectoryExists(full))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    code,
                    $"Source folder '{configured}' does not exist: {full}"
                )
            );
        }
    }

    private void ValidateRenderer(GeneratorConfig config, List<ConfigIssue> issues)
    {
        var engine = config.Renderer?.Engine;
        if (
            !string.IsNullOrWhiteSpace(engine)
            && !engine.Equals("liquid", StringComparison.OrdinalIgnoreCase)
        )
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "REND001",
                    $"Renderer engine '{engine}' is not recognized; expected 'liquid'."
                )
            );
        }
    }

    private void ValidateMarkdown(GeneratorConfig config, List<ConfigIssue> issues)
    {
        var hl = config.Markdown?.Highlighting;
        if (hl == null)
        {
            return;
        }

        if (!AllowedHighlighters.Contains(hl.Provider))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Error,
                    "MD001",
                    $"Unknown highlighting provider '{hl.Provider}'. Allowed: builtin, passthrough."
                )
            );
        }

        if (
            !string.IsNullOrWhiteSpace(hl.Theme)
            && !BuiltInThemes.Contains(hl.Theme)
            && !hl.CustomTheme.ContainsKey(hl.Theme)
            && hl.Enabled
        )
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "MD002",
                    $"Highlighting theme '{hl.Theme}' is not built-in ('light'/'dark') and has no 'customTheme' entry."
                )
            );
        }

        if (hl.Enabled && hl.GenerateCss && string.IsNullOrWhiteSpace(hl.CssFile))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "MD003",
                    "'generateCss' is enabled but 'cssFile' is empty; the stylesheet name is required."
                )
            );
        }
    }

    private void ValidateImages(GeneratorConfig config, List<ConfigIssue> issues)
    {
        var img = config.Assets?.Images;
        if (img == null)
        {
            return;
        }

        if (!AllowedImageProcessors.Contains(img.Processor))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Error,
                    "IMG001",
                    $"Unknown image processor '{img.Processor}'. Allowed: imagesharp, passthrough."
                )
            );
        }

        if (img.Quality < 0 || img.Quality > 100)
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "IMG002",
                    $"Image quality {img.Quality} is outside the recommended 0-100 range."
                )
            );
        }

        if (img.Enabled && img.Sizes != null && img.Sizes.Any(s => s <= 0))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "IMG003",
                    "One or more 'sizes' values are not positive integers."
                )
            );
        }

        if (img.Enabled && (img.Sizes == null || img.Sizes.Count == 0) && img.GenerateWebP)
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Info,
                    "IMG004",
                    "No responsive 'sizes' configured; only the original image will be emitted."
                )
            );
        }
    }

    private void ValidateCache(GeneratorConfig config, List<ConfigIssue> issues)
    {
        if (config.Cache == null)
        {
            return;
        }

        if (
            Path.IsPathRooted(config.Cache.Directory)
            || config.Cache.Directory.Contains("..", StringComparison.Ordinal)
        )
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "CACHE001",
                    "'cache.directory' should be a project-relative path, not an absolute/root path."
                )
            );
        }
    }

    private void ValidateRedirects(GeneratorConfig config, List<ConfigIssue> issues)
    {
        if (config.Redirects == null || !config.Redirects.Enabled)
        {
            return;
        }

        for (int i = 0; i < config.Redirects.Rules.Count; i++)
        {
            var rule = config.Redirects.Rules[i];
            if (string.IsNullOrWhiteSpace(rule.From))
            {
                issues.Add(
                    new ConfigIssue(
                        DiagnosticSeverity.Warning,
                        "RED001",
                        $"Redirect rule #{i + 1} has no 'from' path."
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(rule.To))
            {
                issues.Add(
                    new ConfigIssue(
                        DiagnosticSeverity.Warning,
                        "RED002",
                        $"Redirect rule for '{rule.From}' has no 'to' target."
                    )
                );
            }
        }
    }

    private void ValidateCollections(GeneratorConfig config, List<ConfigIssue> issues)
    {
        foreach (var (name, settings) in config.Collections ?? [])
        {
            if (string.IsNullOrWhiteSpace(settings.Source))
            {
                issues.Add(
                    new ConfigIssue(
                        DiagnosticSeverity.Warning,
                        "COL001",
                        $"Collection '{name}' has no 'source' folder configured."
                    )
                );
            }
        }
    }

    private void ValidateRss(GeneratorConfig config, List<ConfigIssue> issues)
    {
        if (config.Rss == null || !config.Rss.Enabled)
        {
            return;
        }

        if (!config.Collections.ContainsKey(config.Rss.Collection))
        {
            issues.Add(
                new ConfigIssue(
                    DiagnosticSeverity.Warning,
                    "RSS001",
                    $"RSS feed is enabled but references collection '{config.Rss.Collection}', which is not defined."
                )
            );
        }
    }
}
