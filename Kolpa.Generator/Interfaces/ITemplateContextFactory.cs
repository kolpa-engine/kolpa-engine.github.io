using Fluid;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Factory that constructs and registers the Fluid TemplateContext mapping.
/// </summary>
public interface ITemplateContextFactory
{
    /// <summary>
    /// Constructs a TemplateContext configured with site values and layout references.
    /// </summary>
    TemplateContext CreateContext(SiteContext siteContext, string layoutsDir);
}
