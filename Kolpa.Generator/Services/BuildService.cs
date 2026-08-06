using System.Diagnostics;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Services;

/// <summary>
/// Pipeline runner service that executes registered IBuildStages sequentially.
/// </summary>
public class BuildService
{
    private readonly IEnumerable<IBuildStage> _stages;
    private readonly ILogger _logger;
    private readonly string _projectDir;
    private readonly ISystemClock _systemClock;

    public BuildService(
        IEnumerable<IBuildStage> stages,
        ILogger logger,
        ISystemClock systemClock,
        string projectDir)
    {
        _stages = stages;
        _logger = logger;
        _systemClock = systemClock;
        _projectDir = projectDir;
    }

    public async Task<bool> ExecuteBuildAsync(string configPath, bool watchMode = false)
    {
        var stopwatch = Stopwatch.StartNew();
        var context = new BuildContext(_projectDir);
        context.Metadata["BuildDate"] = _systemClock.UtcNow;
        context.Metadata["WatchMode"] = watchMode;

        _logger.LogInfo("\nKolpa SSG Engine - Running Build Pipeline");
        _logger.LogInfo("========================================");

        bool pipelineFailed = false;

        foreach (var stage in _stages)
        {
            var stageStopwatch = Stopwatch.StartNew();
            _logger.LogVerbose($"Starting stage: '{stage.Name}'...");

            try
            {
                await stage.ExecuteAsync(context);
                stageStopwatch.Stop();
                _logger.LogVerbose($"Finished stage '{stage.Name}' in {stageStopwatch.ElapsedMilliseconds}ms.");

                // Break on fatal stage errors
                if (context.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    pipelineFailed = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(DiagnosticSeverity.Error, $"Fatal exception executing stage '{stage.Name}': {ex.Message}", stage.Name);
                pipelineFailed = true;
                break;
            }
        }

        stopwatch.Stop();

        // 1. Report Diagnostics Traces
        _logger.LogInfo("\nDiagnostics Summary:");
        _logger.LogInfo("--------------------");

        foreach (var diag in context.Diagnostics)
        {
            var formatted = $"[{diag.StageName}] {diag.Message}";
            switch (diag.Severity)
            {
                case DiagnosticSeverity.Error:
                    _logger.LogError(formatted);
                    break;
                case DiagnosticSeverity.Warning:
                    _logger.LogWarn(formatted);
                    break;
                case DiagnosticSeverity.Info:
                default:
                    _logger.LogVerbose(formatted);
                    break;
            }
        }

        // 2. Build Summary output
        _logger.LogInfo("\nBuild Metrics:");
        _logger.LogInfo("--------------");

        var pagesCount = context.GeneratedFiles.Count;
        var assetsCount = context.Assets.Count;
        var warnings = context.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
        var errors = context.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);

        _logger.LogInfo($"  Duration:      {stopwatch.ElapsedMilliseconds}ms");
        _logger.LogInfo($"  Pages:         {pagesCount} generated");
        _logger.LogInfo($"  Assets:        {assetsCount} processed");
        _logger.LogInfo($"  Warnings:      {warnings}");
        _logger.LogInfo($"  Errors:        {errors}");

        if (errors > 0 || pipelineFailed)
        {
            _logger.LogError("Build completed with fatal errors.");
            return false;
        }

        _logger.LogInfo("Build completed successfully.");
        return true;
    }
}
