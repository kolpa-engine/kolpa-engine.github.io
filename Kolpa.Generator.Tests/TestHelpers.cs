using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kolpa.Generator.Tests;

public sealed class NullLogger : ILogger
{
    public void LogInfo(string message) { }

    public void LogWarn(string message) { }

    public void LogError(string message) { }

    public void LogVerbose(string message) { }
}

public static class TestHelpers
{
    /// <summary>Creates a temp project directory with a config file and returns its root.</summary>
    public static string CreateTempProject(string configJson)
    {
        var root = Path.Combine(Path.GetTempPath(), "kolpa-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "pages"));
        Directory.CreateDirectory(Path.Combine(root, "layouts"));
        File.WriteAllText(Path.Combine(root, "config.json"), configJson);
        return root;
    }

    public static GeneratorConfig ConfigFromJson(string json)
    {
        return JsonSerializer.Deserialize<GeneratorConfig>(json) ?? new GeneratorConfig();
    }

    /// <summary>Generates a solid-color PNG of the given size and returns its path.</summary>
    public static string CreatePng(string dir, int width, int height, string name = "sample.png")
    {
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name);
        using var image = new Image<Rgba32>(width, height);
        image.Mutate(x => x.BackgroundColor(Color.DarkSlateBlue));
        image.SaveAsPng(path);
        return path;
    }

    public static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "kolpa-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
