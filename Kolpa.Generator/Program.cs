using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Pipeline;
using Kolpa.Generator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kolpa.Generator;

public static class Program
{
    private static FileSystemWatcher? _watcher;
    private static IServiceProvider? _serviceProvider;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].ToLowerInvariant();

        // Parse options
        bool verbose = args.Contains("--verbose") || args.Contains("-v");
        bool watch =
            args.Contains("--watch")
            || args.Contains("-w")
            || (command == "serve" && !args.Contains("--no-watch"));

        int port = 5000;
        int portIndex = Array.FindIndex(
            args,
            arg =>
                arg.Equals("--port", StringComparison.OrdinalIgnoreCase)
                || arg.Equals("-p", StringComparison.OrdinalIgnoreCase)
        );
        if (portIndex != -1 && portIndex + 1 < args.Length)
        {
            int.TryParse(args[portIndex + 1], out port);
        }

        var projectDir = Directory.GetCurrentDirectory();

        // Check for explicit --dir or -d option
        int dirIndex = Array.FindIndex(
            args,
            arg =>
                arg.Equals("--dir", StringComparison.OrdinalIgnoreCase)
                || arg.Equals("-d", StringComparison.OrdinalIgnoreCase)
        );
        if (dirIndex != -1 && dirIndex + 1 < args.Length)
        {
            projectDir = Path.GetFullPath(args[dirIndex + 1]);
        }
        else
        {
            // Check for positional argument (non-flag) after the command
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (
                        args[i].Equals("--port", StringComparison.OrdinalIgnoreCase)
                        || args[i].Equals("-p", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        i++; // skip next arg (the value)
                    }
                    continue;
                }
                projectDir = Path.GetFullPath(args[i]);
                break;
            }
        }

        // Check if executed inside generator project subfolder as fallback
        if (
            !Directory.Exists(Path.Combine(projectDir, "pages"))
            && File.Exists(Path.Combine(projectDir, "..", "config.json"))
            && !File.Exists(Path.Combine(projectDir, "config.json"))
        )
        {
            projectDir = Path.GetFullPath(Path.Combine(projectDir, ".."));
        }

        var configPath = Path.Combine(projectDir, "config.json");

        // 1. Initialize Dependency Injection Container
        _serviceProvider = ConfigureServices(projectDir, configPath, verbose);
        var logger = _serviceProvider.GetRequiredService<ILogger>();

        switch (command)
        {
            case "build":
                var buildResult = await ExecuteBuildAsync(configPath, watch);
                if (buildResult && watch)
                {
                    StartWatching(projectDir, configPath, logger);
                    if (Console.IsInputRedirected)
                    {
                        logger.LogInfo("Running watch mode in background...");
                        await Task.Delay(Timeout.Infinite);
                    }
                    else
                    {
                        logger.LogInfo("Press any key to exit watch mode...");
                        Console.ReadKey(true);
                    }
                }
                return buildResult ? 0 : 1;

            case "clean":
                return ExecuteClean(configPath) ? 0 : 1;

            case "serve":
                return await ExecuteServeAsync(projectDir, configPath, port, watch);

            case "help":
            case "--help":
            case "-h":
                PrintUsage();
                return 0;

            default:
                logger.LogError($"Unknown command: {command}");
                PrintUsage();
                return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Kolpa Engine Static Site Generator CLI");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  kolpa <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  build      Generate the static website.");
        Console.WriteLine("  clean      Delete generated build output.");
        Console.WriteLine("  serve      Launch a development HTTP server.");
        Console.WriteLine("  help       Show this help manual.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --verbose, -v    Print detailed execution logs during steps.");
        Console.WriteLine("  --watch, -w      Rebuild the site automatically when files change.");
        Console.WriteLine(
            "  --no-watch       Disable rebuilding the site automatically on changes when serving."
        );
        Console.WriteLine("  --port, -p <n>   Specify the port for dev server (default: 5000).");
        Console.WriteLine(
            "  --dir, -d <path> Specify the project directory (default: current directory)."
        );
        Console.WriteLine();
    }

    private static IServiceProvider ConfigureServices(
        string projectDir,
        string configPath,
        bool verbose
    )
    {
        var services = new ServiceCollection();

        // Load configuration to register settings
        GeneratorConfig config;
        try
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<GeneratorConfig>(json) ?? new GeneratorConfig();
            }
            else
            {
                config = new GeneratorConfig();
            }
        }
        catch
        {
            config = new GeneratorConfig();
        }

        // register framework dependencies
        services.AddSingleton(config);
        services.AddSingleton<IFileSystem, PhysicalFileSystem>();
        services.AddSingleton<ILogger>(new ConsoleLogger(verbose));
        services.AddSingleton<ISystemClock, SystemClock>();

        //  load plugins dynamically via explicit registration APIs
        var plugins = new List<IEnginePlugin>
        {
            new CoreEnginePlugin(projectDir, configPath),
            new SitemapPlugin(),
            new RssPlugin(),
        };

        foreach (var plugin in plugins)
        {
            plugin.ConfigureServices(services, config);
        }

        // register pipeline runner BuildService
        services.AddSingleton(provider => new BuildService(
            provider.GetServices<IBuildStage>(),
            provider.GetRequiredService<ILogger>(),
            provider.GetRequiredService<ISystemClock>(),
            projectDir,
            provider.GetRequiredService<ICacheService>()
        ));

        return services.BuildServiceProvider();
    }

    private static async Task<bool> ExecuteBuildAsync(string configPath, bool watchMode = false)
    {
        if (_serviceProvider == null)
            return false;
        var buildService = _serviceProvider.GetRequiredService<BuildService>();
        return await buildService.ExecuteBuildAsync(configPath, watchMode);
    }

    private static bool ExecuteClean(string configPath)
    {
        if (_serviceProvider == null)
            return false;
        var config = _serviceProvider.GetRequiredService<GeneratorConfig>();
        var fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();
        var logger = _serviceProvider.GetRequiredService<ILogger>();

        var outputDir = Path.GetFullPath(config.Paths.Output);
        try
        {
            fileSystem.DeleteDirectory(outputDir, true);
            logger.LogInfo($"Clean completed. Wiped target output folder: {outputDir}");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Clean failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<int> ExecuteServeAsync(
        string projectDir,
        string configPath,
        int port,
        bool watch
    )
    {
        if (_serviceProvider == null)
            return 1;
        var config = _serviceProvider.GetRequiredService<GeneratorConfig>();
        var logger = _serviceProvider.GetRequiredService<ILogger>();

        var success = await ExecuteBuildAsync(configPath, watch);
        if (!success)
        {
            logger.LogWarn("Site build failed. Launching preview with existing outputs.");
        }

        try
        {
            var outputDir = Path.GetFullPath(Path.Combine(projectDir, config.Paths.Output));

            var server = new DevServer(outputDir, port);
            server.Start();

            if (watch)
            {
                StartWatching(projectDir, configPath, logger);
            }

            if (Console.IsInputRedirected)
            {
                logger.LogInfo("Running dev server in background...");
                await Task.Delay(Timeout.Infinite);
            }
            else
            {
                logger.LogInfo("Press [Ctrl+C] or any key to stop server...");
                Console.ReadKey(true);
            }

            server.Stop();
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError($"Serve command execution failed: {ex.Message}");
            return 1;
        }
    }

    private static void StartWatching(string projectDir, string configPath, ILogger logger)
    {
        logger.LogInfo("[WATCH] File system watcher enabled. Scanning for changes...");

        _watcher = new FileSystemWatcher(projectDir)
        {
            IncludeSubdirectories = true,
            NotifyFilter =
                NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
            Filter = "*.*",
        };

        DateTime lastRebuild = DateTime.MinValue;
        var rebuildLock = new object();

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            var normalizedPath = e.FullPath.Replace('\\', '/');
            // Ignore compiler output, caches, git, and local agent files
            if (
                normalizedPath.Contains("/dist/")
                || normalizedPath.Contains("/bin/")
                || normalizedPath.Contains("/obj/")
                || normalizedPath.Contains("/.git/")
                || normalizedPath.Contains("/.agents/")
                || normalizedPath.Contains("/.gemini/")
                || normalizedPath.Contains("/.generator-cache/")
                || normalizedPath.Contains("/.generator-cache")
            )
            {
                return;
            }

            lock (rebuildLock)
            {
                if (DateTime.UtcNow - lastRebuild < TimeSpan.FromMilliseconds(500))
                {
                    return;
                }
                lastRebuild = DateTime.UtcNow;
            }

            logger.LogInfo($"\n[WATCH] Change detected: {e.Name} ({e.ChangeType}). Rebuilding...");
            Task.Run(async () =>
            {
                var buildSuccess = await ExecuteBuildAsync(configPath, true);
                if (buildSuccess)
                {
                    DevServer.BroadcastReload();
                }
            });
        }

        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += (s, ev) => OnChanged(s, ev);

        _watcher.EnableRaisingEvents = true;
    }
}

/// <summary>
/// Core pipeline configurations plugin, registering standard stages, parsers, and services.
/// </summary>
public class CoreEnginePlugin(string projectDir, string configPath) : IEnginePlugin
{
    private readonly string _projectDir = projectDir;
    private readonly string _configPath = configPath;

    public string Name => "Core Engine Config Plugin";

    public void ConfigureServices(IServiceCollection services, GeneratorConfig config)
    {
        // register content parsers
        services.AddSingleton<IContentParser, MarkdownContentParser>();
        services.AddSingleton<IContentParser, LiquidContentParser>();

        // register folder providers
        services.AddSingleton<IContentProvider, FolderContentProvider>();

        // markdown + highlighting + image + cache services
        services.AddSingleton<ICacheService>(provider => new CacheService(
            provider.GetRequiredService<GeneratorConfig>(),
            _projectDir
        ));

        services.AddSingleton<IMarkdownRenderer>(provider => new MarkdownRenderer(
            provider.GetRequiredService<GeneratorConfig>()
        ));

        services.AddSingleton<ICodeHighlighter>(provider =>
        {
            var cfg = provider.GetRequiredService<GeneratorConfig>();
            var providerName = cfg.Markdown.Highlighting.Provider ?? "builtin";
            return providerName.Equals("passthrough", StringComparison.OrdinalIgnoreCase)
                ? new PassthroughCodeHighlighter()
                : new BuiltinSyntaxHighlighter(cfg.Markdown.Highlighting.CssPrefix);
        });

        services.AddSingleton<IImageProcessor>(provider =>
        {
            var cfg = provider.GetRequiredService<GeneratorConfig>();
            var processor = cfg.Assets.Images.Processor ?? "imagesharp";
            return processor.Equals("passthrough", StringComparison.OrdinalIgnoreCase)
                ? new PassthroughImageProcessor()
                : new ImageSharpProcessor();
        });

        // setup layouts path relative to config
        string layoutsDir = Path.GetFullPath(Path.Combine(_projectDir, config.Paths.Layouts));
        services.AddSingleton<ITemplateRenderer>(new FluidTemplateRenderer(layoutsDir));
        services.AddSingleton<ITemplateContextFactory, FluidTemplateContextFactory>();

        // register services
        services.AddSingleton(provider => new ContentService(
            provider.GetRequiredService<IContentProvider>(),
            provider.GetRequiredService<ILogger>(),
            _projectDir
        ));

        services.AddSingleton(provider => new TemplateService(
            provider.GetRequiredService<ITemplateRenderer>(),
            provider.GetRequiredService<ITemplateContextFactory>(),
            provider.GetRequiredService<ILogger>(),
            layoutsDir
        ));

        services.AddSingleton<IRouteGenerator, RouteService>();
        services.AddSingleton<OutputService>();

        // register pipeline stages in sequential order
        services.AddSingleton<IBuildStage, LoadConfigurationStage>(
            provider => new LoadConfigurationStage(
                provider.GetRequiredService<IFileSystem>(),
                _configPath
            )
        );
        services.AddSingleton<IBuildStage, DiscoverFilesStage>();
        services.AddSingleton<IBuildStage, LoadContentStage>();
        services.AddSingleton<IBuildStage, ProcessMarkdownStage>();
        services.AddSingleton<IBuildStage, HighlightCodeStage>();
        services.AddSingleton<IBuildStage, LoadDataStage>();
        services.AddSingleton<IBuildStage, BuildCollectionsStage>();
        services.AddSingleton<IBuildStage, ResolveRoutesStage>();
        services.AddSingleton<IBuildStage, BuildTagArchivesStage>();
        services.AddSingleton<IBuildStage, BuildNavigationStage>();
        services.AddSingleton<IBuildStage, GenerateMetadataStage>();
        services.AddSingleton<IBuildStage, RenderTemplatesStage>();
        services.AddSingleton<IBuildStage, LiveReloadInjectionStage>();
        services.AddSingleton<IBuildStage, WriteOutputStage>();
        services.AddSingleton<IBuildStage, ProcessImagesStage>();
        services.AddSingleton<IBuildStage, OptimizeAssetsStage>();
        services.AddSingleton<IBuildStage, RunPostBuildStage>();
    }
}

/// <summary>
/// Reusable sitemap plugin implementing the IEnginePlugin interface.
/// </summary>
public class SitemapPlugin : IEnginePlugin, IBuildStep
{
    private GeneratorConfig? _config;

    public string Name => "Sitemap Generator Plugin";

    public void ConfigureServices(IServiceCollection services, GeneratorConfig config)
    {
        // Register this instance as a post-build step sitemapper
        _config = config;
        services.AddSingleton<IBuildStep>(this);
    }

    public async Task ExecuteAsync(Models.SiteContext siteContext, string outputDir)
    {
        var config = _config;
        if (config == null || string.IsNullOrWhiteSpace(config.Site.Url))
        {
            return;
        }

        var baseUrl = config.Site.Url.TrimEnd('/');
        var urls = siteContext
            .Urls.Select(url => baseUrl + EnsureLeadingSlash(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(url => url, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var urlset = string.Join(
            "\n",
            urls.Select(url =>
                $"  <url>\n    <loc>{System.Security.SecurityElement.Escape(url)}</loc>\n  </url>"
            )
        );

        var sitemapFile = Path.Combine(outputDir, "sitemap.xml");
        var xml =
            $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n{urlset}\n</urlset>";
        await File.WriteAllTextAsync(sitemapFile, xml);
    }

    private static string EnsureLeadingSlash(string url)
    {
        return url.StartsWith('/') ? url : "/" + url;
    }
}
