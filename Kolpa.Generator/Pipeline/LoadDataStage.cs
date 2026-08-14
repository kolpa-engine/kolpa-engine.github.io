using System.Text.Json;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that registers structured JSON configurations in the global DataRegistry context.
/// </summary>
public class LoadDataStage(IFileSystem fileSystem) : IBuildStage
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public string Name => "Load Data";

  public async Task ExecuteAsync(BuildContext context)
    {
        var root = context.Project.RootPath;
        var dataPath = Path.Combine(root, context.Config.Paths.Data);

        if (!_fileSystem.DirectoryExists(dataPath))
        {
            return;
        }

        var files = _fileSystem.EnumerateFiles(dataPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var key = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            try
            {
                var json = await _fileSystem.ReadFileAsync(file);
                using var document = JsonDocument.Parse(json);
                var converted = ConvertJsonElement(document.RootElement.Clone());
                if (converted != null)
                {
                    context.DataRegistry[key] = converted;
                    context.AddDiagnostic(DiagnosticSeverity.Info, $"Registered global data variable 'data.{key}'", Name);
                }
            }
            catch (Exception ex)
            {
                context.AddDiagnostic(DiagnosticSeverity.Warning, $"Failed loading data file {file}: {ex.Message}", Name);
            }
        }
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in element.EnumerateObject())
                {
                    var convertedVal = ConvertJsonElement(prop.Value);
                    if (convertedVal != null)
                    {
                        dict[prop.Name] = convertedVal;
                    }
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    var convertedVal = ConvertJsonElement(item);
                    if (convertedVal != null)
                    {
                        list.Add(convertedVal);
                    }
                }
                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt64(out long l)) return l;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
            default:
                return null;
        }
    }
}
