using Microsoft.Extensions.DependencyInjection;
using Kolpa.Generator.Config;

namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Hook plugin allowing custom assemblies to register services, processors, and template features.
/// </summary>
public interface IEnginePlugin
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services, GeneratorConfig config);
}
