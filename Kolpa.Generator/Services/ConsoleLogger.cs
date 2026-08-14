using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// Implements console output logging with color styles and verbosity filtering.
/// </summary>
public class ConsoleLogger(bool verbose = false) : ILogger
{
    private readonly bool _verbose = verbose;

  public void LogInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void LogWarn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ResetColor();
    }

    public void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public void LogVerbose(string message)
    {
        if (_verbose)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[VERBOSE] {message}");
            Console.ResetColor();
        }
    }
}
