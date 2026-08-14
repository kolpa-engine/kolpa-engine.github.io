using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Tests;

public class ImageProcessorTests
{
    [Fact]
    public async Task ImageSharp_Produces_Primary_And_Responsive_WebP_With_Original()
    {
        var dir = TestHelpers.TempDir();
        var png = TestHelpers.CreatePng(dir, 2000, 1000);

        var options = new ImageProcessOptions
        {
            GenerateWebP = true,
            GenerateAvif = false,
            MaxWidth = 1920,
            PreserveOriginal = true,
            Quality = 85,
            Sizes = new List<int> { 320, 640, 1280, 1920 },
        };

        var result = await new ImageSharpProcessor().ProcessAsync(png, options);

        // Original preserved.
        Assert.Contains(result.Variants, v => v.IsOriginal && v.Format == "png");

        // Primary webp at the capped max width (2000 -> 1920).
        var primary = result.Variants.First(v =>
            v.FileName.EndsWith(".webp") && !v.FileName.Contains('-')
        );
        Assert.Equal(1920, primary.Width);

        // Responsive webp variants at 320/640/1280 (1920 is not < 1920, so excluded).
        Assert.Contains(result.Variants, v => v.FileName == "sample-320.webp");
        Assert.Contains(result.Variants, v => v.FileName == "sample-640.webp");
        Assert.Contains(result.Variants, v => v.FileName == "sample-1280.webp");

        // No upscaling: no variant wider than the source.
        Assert.All(result.Variants, v => Assert.True(v.Width <= 2000));
    }

    [Fact]
    public async Task ImageSharp_Does_Not_Upscale_Small_Images()
    {
        var dir = TestHelpers.TempDir();
        var png = TestHelpers.CreatePng(dir, 100, 50);

        var options = new ImageProcessOptions
        {
            GenerateWebP = true,
            MaxWidth = 1920,
            PreserveOriginal = true,
            Sizes = new List<int> { 320, 640 },
        };

        var result = await new ImageSharpProcessor().ProcessAsync(png, options);

        var primary = result.Variants.First(v => v.FileName == "sample.webp");
        Assert.Equal(100, primary.Width);
        // 320/640 are both >= 100, so no responsive variants are generated.
        Assert.DoesNotContain(result.Variants, v => v.FileName.Contains("-320.webp"));
    }

    [Fact]
    public async Task Passthrough_Preserves_Original_Only()
    {
        var dir = TestHelpers.TempDir();
        var png = TestHelpers.CreatePng(dir, 640, 480);

        var result = await new PassthroughImageProcessor().ProcessAsync(
            png,
            new ImageProcessOptions()
        );

        Assert.Single(result.Variants);
        Assert.True(result.Variants[0].IsOriginal);
        Assert.Equal("png", result.Variants[0].Format);
    }

    [Fact]
    public void Plan_Computes_Responsive_Names_Without_Upscaling()
    {
        var cfg = new Kolpa.Generator.Config.ImageSettings
        {
            GenerateWebP = true,
            MaxWidth = 1920,
            Sizes = new List<int> { 320, 640, 1280, 1920 },
        };

        var names = ImagePlan.ComputeVariantFileNames("features/hero.png", 2000, 1000, cfg);

        Assert.Contains("hero.png", names); // preserved original
        Assert.Contains("hero.webp", names); // primary webp
        Assert.Contains("hero-1280.webp", names); // responsive
        Assert.DoesNotContain("hero-1920.webp", names); // 1920 !< 1920
    }
}
