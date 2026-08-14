using System.Text.Json;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that discovers and deserializes config.json file into BuildContext.
/// </summary>
public class LoadConfigurationStage(
    IFileSystem fileSystem,
    string configPath,
    ConfigValidator validator,
    string projectRoot
) : IBuildStage
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly string _configPath = configPath;
    private readonly ConfigValidator _validator = validator;
    private readonly string _projectRoot = projectRoot;

    public string Name => "Load Configuration";

    public async Task ExecuteAsync(BuildContext context)
    {
        if (!_fileSystem.FileExists(_configPath))
        {
            context.AddDiagnostic(
                DiagnosticSeverity.Error,
                $"Configuration file config.json was not found at: {_configPath}",
                Name
            );
            return;
        }

        try
        {
            var json = await _fileSystem.ReadFileAsync(_configPath);
            var config = JsonSerializer.Deserialize<GeneratorConfig>(json);
            if (config != null)
            {
                context.Config = config;
                context.AddDiagnostic(
                    DiagnosticSeverity.Info,
                    $"Loaded website config: '{config.Site.Title}'",
                    Name
                );

                foreach (var issue in _validator.Validate(config))
                {
                    context.AddDiagnostic(issue.Severity, $"[{issue.Code}] {issue.Message}", Name);
                }
            }
            else
            {
                context.AddDiagnostic(
                    DiagnosticSeverity.Error,
                    "Parsed configuration was null.",
                    Name
                );
            }
        }
        catch (Exception ex)
        {
            context.AddDiagnostic(
                DiagnosticSeverity.Error,
                $"Failed to load config.json: {ex.Message}",
                Name
            );
        }
    }
}
