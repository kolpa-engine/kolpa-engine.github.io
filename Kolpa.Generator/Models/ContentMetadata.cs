namespace Kolpa.Generator.Models;

/// <summary>
/// Represents strongly-typed frontmatter and document metadata.
/// </summary>
public class ContentMetadata
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
    public bool Draft { get; set; } = false;
    public DateTime? Date { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Custom metadata properties dictionary for user-defined frontmatter variables.
    /// </summary>
    public Dictionary<string, object> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Indexer allowing custom fields access via metadata["customFieldName"].
    /// </summary>
    public object? this[string key]
    {
        get
        {
            switch (key.ToLowerInvariant())
            {
                case "title": return Title;
                case "description": return Description;
                case "layout": return Layout;
                case "draft": return Draft;
                case "date": return Date;
                case "tags": return Tags;
                case "category": return Category;
                default:
                    return CustomFields.TryGetValue(key, out var val) ? val : null;
            }
        }
        set
        {
            if (value == null) return;
            switch (key.ToLowerInvariant())
            {
                case "title": Title = value.ToString() ?? ""; break;
                case "description": Description = value.ToString() ?? ""; break;
                case "layout": Layout = value.ToString() ?? ""; break;
                case "draft": Draft = value is bool b && b; break;
                case "date": Date = value is DateTime dt ? dt : DateTime.TryParse(value.ToString(), out var parsedDt) ? parsedDt : null; break;
                case "tags":
                    if (value is List<string> tagList) Tags = tagList;
                    else if (value is IEnumerable<object> objEnum) Tags = [.. objEnum.Select(o => o.ToString() ?? "")];
                    break;
                case "category": Category = value.ToString() ?? ""; break;
                default:
                    CustomFields[key] = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Converts metadata into a dictionary representation for Fluid context binding.
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = Title,
            ["description"] = Description,
            ["layout"] = Layout,
            ["draft"] = Draft,
            ["tags"] = Tags,
            ["category"] = Category
        };

        if (Date.HasValue)
        {
            dict["date"] = Date.Value;
        }

        foreach (var kvp in CustomFields)
        {
            dict[kvp.Key] = kvp.Value;
        }

        return dict;
    }
}
