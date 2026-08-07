using Kolpa.Generator.Config;
using Kolpa.Generator.Models;
using Kolpa.Generator.Pipeline;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Tests;

public class AssetMinifierTests
{
    [Fact]
    public void MinifyCss_Strips_Comments_And_Collapses_Whitespace()
    {
        var input = """
            /* header */
            .foo {
                color: red;
                margin: 0 auto;
            }

            /* trailing */
            """;

        var minified = AssetMinifier.MinifyCss(input);

        Assert.DoesNotContain("/*", minified);
        Assert.DoesNotContain("\n", minified);
        Assert.Equal(".foo{color:red;margin:0auto;}", minified.Replace(" ", ""));
    }

    [Fact]
    public void MinifyCss_Preserves_String_Content()
    {
        var input = """
            .a::before {
                content: "hello world";
            }
            """;

        var minified = AssetMinifier.MinifyCss(input);

        Assert.Contains("\"hello world\"", minified);
        Assert.DoesNotContain("content: hello", minified);
    }

    [Fact]
    public void MinifyJs_Strips_Line_And_Block_Comments()
    {
        var input = """
            // leading comment
            var x = 1; // trailing
            /* block
               comment */
            var y = 2;
            """;

        var minified = AssetMinifier.MinifyJs(input);

        Assert.DoesNotContain("//", minified);
        Assert.DoesNotContain("/*", minified);
        Assert.DoesNotContain("comment", minified);
        Assert.DoesNotContain("\n", minified);
        Assert.Contains("varx=1;", minified.Replace(" ", ""));
        Assert.Contains("vary=2;", minified.Replace(" ", ""));
    }

    [Fact]
    public void MinifyJs_Preserves_Strings_With_Url_Like_Content()
    {
        var input = """
            var url = "http://example.com/foo";
            """;

        var minified = AssetMinifier.MinifyJs(input);

        Assert.Contains("\"http://example.com/foo\"", minified);
    }
}

public class OptimizeAssetsStageTests
{
    private static BuildContext CreateContext(
        string root,
        string assetsDir,
        string outputDir,
        AssetProcessingSettings processing
    )
    {
        var config = new GeneratorConfig
        {
            Paths = new PathSettings { Assets = assetsDir, Output = outputDir },
            Assets = new AssetSettings { Processing = processing },
            Markdown = new MarkdownSettings(),
        };

        var context = new BuildContext(root) { Config = config };

        var assetsRoot = Path.Combine(root, assetsDir);
        Directory.CreateDirectory(assetsRoot);
        var cssPath = Path.Combine(assetsRoot, "app.css");
        File.WriteAllText(cssPath, "/* c */\nbody { margin: 0; }");
        var jsPath = Path.Combine(assetsRoot, "app.js");
        File.WriteAllText(jsPath, "// x\nvar a = 1;");
        var fontPath = Path.Combine(assetsRoot, "font.woff2");
        File.WriteAllBytes(fontPath, new byte[] { 1, 2, 3, 4 });

        context.Assets.Add(cssPath);
        context.Assets.Add(jsPath);
        context.Assets.Add(fontPath);

        return context;
    }

    [Fact]
    public async Task Fingerprinting_Renames_And_Emits_Manifest()
    {
        var dir = TestHelpers.TempDir();
        var context = CreateContext(
            dir,
            "assets",
            "dist",
            new AssetProcessingSettings
            {
                Enabled = true,
                MinifyCss = true,
                MinifyJs = true,
                Fingerprint = true,
                ManifestFile = "assets-manifest.json",
                HashLength = 8,
            }
        );

        var stage = new OptimizeAssetsStage(new PassthroughImageProcessor(), new NullLogger());
        await stage.ExecuteAsync(context);

        var output = Path.Combine(dir, "dist", "assets");
        var files = Directory.GetFiles(output).Select(Path.GetFileName).ToList();

        // Fingerprinted names follow name.<hash>.ext and never contain a space.
        Assert.Contains(files, f => f.StartsWith("app.") && f.EndsWith(".css"));
        Assert.Contains(files, f => f.StartsWith("app.") && f.EndsWith(".js"));
        // Non-processable assets keep their original name.
        Assert.Contains("font.woff2", files);

        // Manifest written and maps logical -> hashed URLs.
        var manifestPath = Path.Combine(dir, "dist", "assets-manifest.json");
        Assert.True(File.Exists(manifestPath));
        var manifest = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(manifestPath)
        );
        Assert.NotNull(manifest);
        Assert.StartsWith("/assets/app.", manifest["app.css"]);
        Assert.EndsWith(".css", manifest["app.css"]);
        Assert.EndsWith(".js", manifest["app.js"]);
    }

    [Fact]
    public async Task Minified_Output_Is_Smaller()
    {
        var dir = TestHelpers.TempDir();
        var context = CreateContext(
            dir,
            "assets",
            "dist",
            new AssetProcessingSettings
            {
                Enabled = true,
                MinifyCss = true,
                MinifyJs = true,
                Fingerprint = false,
            }
        );

        var stage = new OptimizeAssetsStage(new PassthroughImageProcessor(), new NullLogger());
        await stage.ExecuteAsync(context);

        var cssOut = Path.Combine(dir, "dist", "assets", "app.css");
        var cssContent = File.ReadAllText(cssOut);
        Assert.DoesNotContain("/*", cssContent);
        Assert.DoesNotContain("\n", cssContent);
        Assert.Contains("body{margin:0;}", cssContent.Replace(" ", ""));
    }

    [Fact]
    public async Task Disabled_Processing_Copies_Assets_Unchanged()
    {
        var dir = TestHelpers.TempDir();
        var context = CreateContext(
            dir,
            "assets",
            "dist",
            new AssetProcessingSettings { Enabled = false }
        );

        var stage = new OptimizeAssetsStage(new PassthroughImageProcessor(), new NullLogger());
        await stage.ExecuteAsync(context);

        var cssOut = Path.Combine(dir, "dist", "assets", "app.css");
        Assert.True(File.Exists(cssOut));
        Assert.Contains("/* c */", File.ReadAllText(cssOut));

        // No manifest emitted when processing is disabled.
        var manifestPath = Path.Combine(dir, "dist", "assets-manifest.json");
        Assert.False(File.Exists(manifestPath));
    }
}
