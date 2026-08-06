using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Contract interface representing a single execution step within the generator build pipeline.
/// </summary>
public interface IBuildStage
{
    /// <summary>
    /// Friendly name of this build stage for reports.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the stage operations, reading from and writing to the BuildContext.
    /// </summary>
    Task ExecuteAsync(BuildContext context);
}
