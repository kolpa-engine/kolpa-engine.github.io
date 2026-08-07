using Kolpa.Generator.Config;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kolpa.Generator.Tests;

public class RedirectsPluginTests
{
    private static GeneratorConfig CreateConfig(
        (string from, string to, bool permanent)[]? rules = null,
        bool withNotFound = true
    )
    {
        return new GeneratorConfig
        {
            Redirects = new RedirectSettings
            {
                Enabled = true,
                Rules = (rules ?? Array.Empty<(string, string, bool)>())
                    .Select(r => new RedirectRule
                    {
                        From = r.Item1,
                        To = r.Item2,
                        Permanent = r.Item3,
                    })
                    .ToList(),
            },
            NotFound = new NotFoundSettings
            {
                Enabled = withNotFound,
                Output = "404.html",
                Title = "Not Found",
                Body = "<h1>404</h1>",
            },
        };
    }

    private static RedirectsPlugin CreatePlugin(GeneratorConfig config)
    {
        var plugin = new RedirectsPlugin();
        plugin.ConfigureServices(new ServiceCollection(), config);
        return plugin;
    }

    [Fact]
    public async Task Writes_Meta_Refresh_Redirect_For_Folder_Route()
    {
        var root = TestHelpers.TempDir();
        var plugin = CreatePlugin(CreateConfig([("/old-page/", "/new-page/", true)]));

        await plugin.ExecuteAsync(new SiteContext(), root);

        var file = Path.Combine(root, "old-page", "index.html");
        Assert.True(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.Contains("http-equiv=\"refresh\"", content);
        Assert.Contains("url=/new-page/", content);
        Assert.Contains("rel=\"canonical\"", content);
        Assert.Contains("location.replace(\"/new-page/\")", content);
    }

    [Fact]
    public async Task Emits_Canonical_For_External_Target()
    {
        var root = TestHelpers.TempDir();
        var plugin = CreatePlugin(CreateConfig([("/gone.html", "https://example.com/new", false)]));

        await plugin.ExecuteAsync(new SiteContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "gone.html"));
        Assert.Contains("https://example.com/new", content);
        Assert.Contains("content=\"0; url=https://example.com/new\"", content);
    }

    [Fact]
    public async Task Writes_Fallback_404_When_Missing()
    {
        var root = TestHelpers.TempDir();
        var plugin = CreatePlugin(CreateConfig(withNotFound: true));

        await plugin.ExecuteAsync(new SiteContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "404.html"));
        Assert.Contains("Not Found", content);
        Assert.Contains("<h1>404</h1>", content);
        Assert.Contains("noindex", content);
    }

    [Fact]
    public async Task Does_Not_Overwrite_Existing_404()
    {
        var root = TestHelpers.TempDir();
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(Path.Combine(root, "404.html"), "<html>custom</html>");
        var plugin = CreatePlugin(CreateConfig(withNotFound: true));

        await plugin.ExecuteAsync(new SiteContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "404.html"));
        Assert.Contains("custom", content);
    }

    [Fact]
    public async Task Skips_Redirects_When_Disabled()
    {
        var root = TestHelpers.TempDir();
        var config = CreateConfig([("/a/", "/b/", true)]);
        config.Redirects.Enabled = false;
        var plugin = CreatePlugin(config);

        await plugin.ExecuteAsync(new SiteContext(), root);

        Assert.False(File.Exists(Path.Combine(root, "a", "index.html")));
    }
}
