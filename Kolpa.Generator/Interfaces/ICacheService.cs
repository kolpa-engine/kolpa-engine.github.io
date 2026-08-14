namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Content-addressed build cache. Results (rendered markdown, processed images,
/// metadata) are stored by content hash so unchanged inputs are not reprocessed.
/// </summary>
public interface ICacheService
{
    /// <summary>Whether the cache is enabled by configuration.</summary>
    bool Enabled { get; }

    /// <summary>Absolute path of the cache root directory.</summary>
    string Directory { get; }

    /// <summary>Stable content hash for a string or byte payload.</summary>
    string ComputeHash(string content);

    string ComputeHash(byte[] content);

    /// <summary>Reads a cached text payload for <paramref name="kind"/> (e.g. "markdown").</summary>
    bool TryReadText(string key, string kind, out string text);

    void StoreText(string key, string kind, string text);

    /// <summary>Reads a cached binary payload for <paramref name="kind"/> (e.g. "images").</summary>
    bool TryReadBytes(string key, string kind, out byte[] bytes);

    void StoreBytes(string key, string kind, byte[] bytes);

    /// <summary>Number of cache hits across this build (for verbose reporting).</summary>
    long Hits { get; }

    /// <summary>Number of cache misses across this build (for verbose reporting).</summary>
    long Misses { get; }
}
