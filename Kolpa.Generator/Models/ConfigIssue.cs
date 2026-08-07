namespace Kolpa.Generator.Models;

/// <summary>
/// A single config/project validation finding reported by the <c>doctor</c> command
/// and surfaced during builds. <c>Code</c> is a stable, searchable identifier such as
/// <c>MD001</c> that callers can quote in docs or scripts.
/// </summary>
public readonly record struct ConfigIssue(DiagnosticSeverity Severity, string Code, string Message);
