namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Defines basic logging outputs for compiler reporting.
/// </summary>
public interface ILogger
{
    void LogInfo(string message);
    void LogWarn(string message);
    void LogError(string message);
    void LogVerbose(string message);
}
