using System.Security.Cryptography;
using System.Text;
using Kolpa.Generator.Config;

namespace Kolpa.Generator.Services;

/// <summary>
/// Shared asset-optimization helpers used by both the metadata stage (which computes the
/// template-facing URL map) and the optimize stage (which writes the files), so the names
/// can never drift apart.
/// </summary>
public static class AssetFingerprint
{
    /// <summary>
    /// Returns the content (minified when applicable) for the given CSS/JS source, or
    /// <c>null</c> if the file is not a processable text asset.
    /// </summary>
    public static string? MinifyContent(
        string asset,
        string ext,
        AssetProcessingSettings processing
    )
    {
        if (!processing.Enabled || (ext != "css" && ext != "js"))
        {
            return null;
        }

        var content = File.ReadAllText(asset);
        if (ext == "css" && processing.MinifyCss)
        {
            return AssetMinifier.MinifyCss(content);
        }
        if (ext == "js" && processing.MinifyJs)
        {
            return AssetMinifier.MinifyJs(content);
        }
        return content;
    }

    /// <summary>
    /// Returns the output-relative path (fingerprinted or original) for an asset.
    /// </summary>
    public static string ResolveOutputPath(
        string relative,
        string? minified,
        AssetProcessingSettings processing
    )
    {
        if (processing.Fingerprint && minified != null)
        {
            return HashedFileName(relative, ContentHash(minified, processing.HashLength));
        }
        return relative;
    }

    private static string HashedFileName(string relative, string hash)
    {
        var dir = Path.GetDirectoryName(relative)?.Replace('\\', '/') ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(relative);
        var ext = Path.GetExtension(relative);
        var hashed = $"{name}.{hash}{ext}";
        return dir.Length > 0 ? $"{dir}/{hashed}" : hashed;
    }

    private static string ContentHash(string content, int length)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(content));
        var hex = Convert.ToHexString(bytes).ToLowerInvariant();
        return hex[..Math.Min(length, hex.Length)];
    }
}
