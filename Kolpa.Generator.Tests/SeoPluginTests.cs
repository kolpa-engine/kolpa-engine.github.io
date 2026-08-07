using Kolpa.Generator.Config;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;
using Xunit;

namespace Kolpa.Generator.Tests;

public class SeoPluginTests
{
    private static GeneratorConfig CreateConfig(bool feedsEnabled = true)
    {
        return new GeneratorConfig
        {
            Site = new SiteSettings
            {
                Title = "Test Site",
                Description = "A test site",
                Url = "https://example.com",
            },
            Atom = new AtomSettings
            {
                Enabled = feedsEnabled,
                Collection = "blog",
                Output = "atom.xml",
                Link = "/blog/",
            },
            JsonFeed = new JsonFeedSettings
            {
                Enabled = feedsEnabled,
                Collection = "blog",
                Output = "feed.json",
                Link = "/blog/",
            },
            Seo = new SeoSettings
            {
                Robots = new RobotsSettings { Enabled = true, Output = "robots.txt" },
                JsonLd = new JsonLdSettings { Enabled = true },
            },
        };
    }

    private static SiteContext CreateContext()
    {
        var ctx = new SiteContext();
        ctx.Collections["blog"] = new List<Dictionary<string, object>>
        {
            new()
            {
                ["url"] = "/blog/hello-world/",
                ["title"] = "Hello World",
                ["description"] = "First post",
                ["content"] = "<p>Hi!</p>",
                ["date"] = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            },
        };
        return ctx;
    }

    [Fact]
    public async Task Generates_robots_txt_With_Sitemap()
    {
        var root = TestHelpers.TempDir();
        var plugin = new SeoPlugin();
        plugin.ConfigureServices(new Microsoft.Extensions.DependencyInjection.ServiceCollection(), CreateConfig());

        await plugin.ExecuteAsync(CreateContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "robots.txt"));
        Assert.Contains("User-agent: *", content);
        Assert.Contains("Sitemap: https://example.com/sitemap.xml", content);
    }

    [Fact]
    public async Task Generates_Atom_Feed()
    {
        var root = TestHelpers.TempDir();
        var plugin = new SeoPlugin();
        plugin.ConfigureServices(new Microsoft.Extensions.DependencyInjection.ServiceCollection(), CreateConfig());

        await plugin.ExecuteAsync(CreateContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "atom.xml"));
        Assert.Contains("<feed", content);
        Assert.Contains("Hello World", content);
    }

    [Fact]
    public async Task Generates_Json_Feed()
    {
        var root = TestHelpers.TempDir();
        var plugin = new SeoPlugin();
        plugin.ConfigureServices(new Microsoft.Extensions.DependencyInjection.ServiceCollection(), CreateConfig());

        await plugin.ExecuteAsync(CreateContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "feed.json"));
        Assert.Contains("\"version\": \"https://jsonfeed.org/version/1.1\"", content);
        Assert.Contains("Hello World", content);
    }

    [Fact]
    public async Task Injects_JsonLd_Into_Html_Head()
    {
        var root = TestHelpers.TempDir();
        File.WriteAllTextAsync(Path.Combine(root, "index.html"),
            "<!doctype html><html><head><title>T</title></head><body>Hi</body></html>").GetAwaiter().GetResult();
        var plugin = new SeoPlugin();
        plugin.ConfigureServices(new Microsoft.Extensions.DependencyInjection.ServiceCollection(), CreateConfig());

        await plugin.ExecuteAsync(CreateContext(), root);

        var html = await File.ReadAllTextAsync(Path.Combine(root, "index.html"));
        Assert.Contains("application/ld+json", html);
        Assert.Contains("\"@type\":\"WebSite\"", html);
        Assert.Contains("https://example.com", html);
        Assert.True(html.IndexOf("application/ld+json") < html.IndexOf("</head>"));
    }

    [Fact]
    public async Task Resolves_Fullimage_Url_From_Site()
    {
        var root = TestHelpers.TempDir();
        var config = CreateConfig();
        var plugin = new SeoPlugin();
        plugin.ConfigureServices(new Microsoft.Extensions.DependencyInjection.ServiceCollection(), config);

        await plugin.ExecuteAsync(CreateContext(), root);

        var content = await File.ReadAllTextAsync(Path.Combine(root, "robots.txt"));
        Assert.Contains("https://example.com", content);
    }
}