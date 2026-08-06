using System.Text;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kolpa.Generator.Services;

/// <summary>
/// Parser that decodes Markdown files, extracting YAML frontmatter and exposing the
/// raw Markdown body. Markdown is rendered to HTML later by the <c>ProcessMarkdownStage</c>
/// so it can be cached and extended without coupling parsing to rendering.
/// </summary>
public class MarkdownContentParser : IContentParser
{
    private readonly IDeserializer _yamlDeserializer;

    public MarkdownContentParser()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase)
            || fileExtension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ContentDocument> ParseAsync(string filePath)
    {
        var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var parsed = new ContentDocument
        {
            Id = Path.GetFileName(filePath),
            Slug = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant(),
            Format = "markdown",
        };

        if (fileContent.StartsWith("---"))
        {
            var endOfFrontmatterIndex = fileContent.IndexOf("---", 3);
            if (endOfFrontmatterIndex > 0)
            {
                var yaml = fileContent.Substring(3, endOfFrontmatterIndex - 3).Trim();
                var markdown = fileContent.Substring(endOfFrontmatterIndex + 3).Trim();

                try
                {
                    var metadata = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
                    if (metadata != null)
                    {
                        foreach (var kvp in metadata)
                        {
                            parsed.Metadata[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"Error parsing YAML frontmatter in {filePath}: {ex.Message}"
                    );
                    Console.ResetColor();
                }

                parsed.Body = markdown;
                return parsed;
            }
        }

        parsed.Body = fileContent;
        return parsed;
    }
}
