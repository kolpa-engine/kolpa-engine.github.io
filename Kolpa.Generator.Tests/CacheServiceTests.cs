using Kolpa.Generator.Config;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Tests;

public class CacheServiceTests
{
    private static CacheService Create(string root)
    {
        var config = new GeneratorConfig
        {
            Cache = new CacheSettings { Enabled = true, Directory = ".generator-cache" },
        };
        return new CacheService(config, root);
    }

    [Fact]
    public void Stores_And_Reads_Text()
    {
        var root = TestHelpers.TempDir();
        var cache = Create(root);

        cache.StoreText("key-1", "markdown", "<p>hello</p>");

        Assert.True(cache.TryReadText("key-1", "markdown", out var text));
        Assert.Equal("<p>hello</p>", text);
    }

    [Fact]
    public void Miss_On_Unknown_Key()
    {
        var root = TestHelpers.TempDir();
        var cache = Create(root);

        Assert.False(cache.TryReadText("nope", "markdown", out _));
    }

    [Fact]
    public void Hash_Is_Stable_And_Content_Derived()
    {
        var root = TestHelpers.TempDir();
        var cache = Create(root);

        Assert.Equal(cache.ComputeHash("abc"), cache.ComputeHash("abc"));
        Assert.NotEqual(cache.ComputeHash("abc"), cache.ComputeHash("abd"));
    }

    [Fact]
    public void Tracks_Hits_And_Misses()
    {
        var root = TestHelpers.TempDir();
        var cache = Create(root);

        cache.StoreText("k", "markdown", "x");
        cache.TryReadText("k", "markdown", out _);
        cache.TryReadText("missing", "markdown", out _);

        Assert.Equal(1, cache.Hits);
        Assert.Equal(1, cache.Misses);
    }

    [Fact]
    public void Disabled_Cache_Never_Reads_Or_Writes()
    {
        var root = TestHelpers.TempDir();
        var config = new GeneratorConfig
        {
            Cache = new CacheSettings { Enabled = false, Directory = ".generator-cache" },
        };
        var cache = new CacheService(config, root);

        Assert.False(cache.Enabled);
        cache.StoreText("k", "markdown", "x");
        Assert.False(cache.TryReadText("k", "markdown", out _));
    }
}
