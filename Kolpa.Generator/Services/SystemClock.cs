using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// System clock wrapping DateTime.UtcNow.
/// </summary>
public class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
