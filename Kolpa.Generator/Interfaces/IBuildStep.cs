using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// A plugin extension step executed in the build pipeline.
/// </summary>
public interface IBuildStep
{
    /// <summary>
    /// Executes additional processing tasks during or after site compile.
    /// </summary>
    Task ExecuteAsync(SiteContext siteContext, string outputDir);
}
