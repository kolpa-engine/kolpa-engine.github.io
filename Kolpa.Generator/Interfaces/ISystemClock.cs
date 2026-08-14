namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Abstraction layer for time management, allowing simulated dates in verification testing.
/// </summary>
public interface ISystemClock
{
    DateTime UtcNow { get; }
}
