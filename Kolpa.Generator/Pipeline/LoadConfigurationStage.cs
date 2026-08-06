using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that discovers and deserializes config.json file into BuildContext.
/// </summary>
public class LoadConfigurationStage : IBuildStage
{
    private readonly IFileSystem _fileSystem;
    private readonly string _configPath;

    public string Name => "Load Configuration";

    public LoadConfigurationStage(IFileSystem fileSystem, string configPath)
    {
        _fileSystem = fileSystem;
        _configPath = configPath;
    }

    public async Task ExecuteAsync(BuildContext context)
    {
        if (!_fileSystem.FileExists(_configPath))
        {
            context.AddDiagnostic(DiagnosticSeverity.Error, $"Configuration file config.json was not found at: {_configPath}", Name);
            return;
        }

        try
        {
            var json = await _fileSystem.ReadFileAsync(_configPath);
            var config = JsonSerializer.Deserialize<GeneratorConfig>(json);
            if (config != null)
            {
                context.Config = config;
                context.AddDiagnostic(DiagnosticSeverity.Info, $"Loaded website config: '{config.Site.Title}'", Name);
            }
            else
            {
                context.AddDiagnostic(DiagnosticSeverity.Error, "Parsed configuration was null.", Name);
            }
        }
        catch (Exception ex)
        {
            context.AddDiagnostic(DiagnosticSeverity.Error, $"Failed to load config.json: {ex.Message}", Name);
        }
    }
}
