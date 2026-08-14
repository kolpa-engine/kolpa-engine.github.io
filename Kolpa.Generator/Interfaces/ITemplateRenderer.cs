namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Service interface that compiles and renders templates (e.g. Liquid templates).
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Renders a template content string using the provided model context.
    /// </summary>
    Task<string> RenderAsync(string templateContent, object context);
}
