using Kolpa.Generator;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kolpa.Generator.Tests;

public class PipelineOrderingTests
{
    private static readonly string[] ExpectedOrder =
    {
        "LoadConfigurationStage",
        "DiscoverFilesStage",
        "LoadContentStage",
        "ProcessMarkdownStage",
        "HighlightCodeStage",
        "LoadDataStage",
        "BuildCollectionsStage",
        "ResolveRoutesStage",
        "BuildTagArchivesStage",
        "BuildNavigationStage",
        "GenerateMetadataStage",
        "RenderTemplatesStage",
        "LiveReloadInjectionStage",
        "WriteOutputStage",
        "ProcessImagesStage",
        "OptimizeAssetsStage",
        "RunPostBuildStage",
    };

    [Fact]
    public void Registers_Stages_In_Expected_Order()
    {
        var config = new GeneratorConfig();
        var projectDir = TestHelpers.CreateTempProject("{}");
        var configPath = Path.Combine(projectDir, "config.json");

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddSingleton<IFileSystem, PhysicalFileSystem>();
        services.AddSingleton<ILogger>(new NullLogger());
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<ICacheService>(_ => new CacheService(config, projectDir));
        services.AddSingleton<IMarkdownRenderer>(_ => new MarkdownRenderer(config));
        services.AddSingleton<ICodeHighlighter>(_ => new BuiltinSyntaxHighlighter("hl-"));
        services.AddSingleton<IImageProcessor>(_ => new ImageSharpProcessor());

        new CoreEnginePlugin(projectDir, configPath).ConfigureServices(services, config);

        using var provider = services.BuildServiceProvider();
        var stages = provider.GetServices<IBuildStage>().ToList();
        var names = stages.Select(s => s.GetType().Name).ToList();

        Assert.Equal(ExpectedOrder, names);
    }
}
