using System.Collections.Concurrent;
using Kolpa.Generator.Config;

namespace Kolpa.Generator.Models;

/// <summary>
/// Root structure for a workspace project configuration.
/// </summary>
public record Project(string RootPath);

/// <summary>
/// Represents a routing target map parsed from inputs.
/// </summary>
public class Route
{
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string RenderedHtml { get; set; } = string.Empty;
    public ContentMetadata Metadata { get; set; } = new();
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Severity level of site diagnostic outputs.
/// </summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// A diagnostic trace message.
/// </summary>
public record Diagnostic(DiagnosticSeverity Severity, string Message, string StageName = "Engine");

/// <summary>
/// Represents a generated output file report.
/// </summary>
public record GeneratedFile(string Path, long SizeInBytes);

/// <summary>
/// Node item inside the dynamic page navigation tree.
/// </summary>
public class NavigationNode
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public List<NavigationNode> Children { get; set; } = new();
}

/// <summary>
/// Results output summarizing execution metrics.
/// </summary>
public class BuildResult
{
    public int PagesGenerated { get; set; } = 0;
    public int AssetsProcessed { get; set; } = 0;
    public long DurationMs { get; set; } = 0;
    public List<Diagnostic> Diagnostics { get; set; } = new();
    public List<GeneratedFile> GeneratedFiles { get; set; } = new();
    public bool Success => Diagnostics.TrueForAll(d => d.Severity != DiagnosticSeverity.Error);
}

/// <summary>
/// Central context state passing through all build stages.
/// </summary>
public class BuildContext(string projectRoot)
{
  public Project Project { get; } = new Project(projectRoot);
  public GeneratorConfig Config { get; set; } = new();

    // Core Registries
    public List<ContentDocument> Documents { get; } = new();
    public Dictionary<string, List<ContentDocument>> Collections { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Templates { get; } = new(StringComparer.OrdinalIgnoreCase); // layoutName -> layoutContent
    public List<string> Assets { get; } = new();
    public List<Route> Routes { get; } = new();
    public Dictionary<string, object> DataRegistry { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<Diagnostic> Diagnostics { get; } = new();
    public List<GeneratedFile> GeneratedFiles { get; } = new();
    public List<NavigationNode> Navigation { get; } = new();

    // Cache registry
    public ConcurrentDictionary<string, object> TemplateCache { get; } = new();

    // Metadata dictionary
    public Dictionary<string, object> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

  public void AddDiagnostic(DiagnosticSeverity severity, string message, string stage)
    {
        Diagnostics.Add(new Diagnostic(severity, message, stage));
    }
}
