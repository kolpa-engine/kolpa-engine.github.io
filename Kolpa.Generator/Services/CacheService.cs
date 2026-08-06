using System.Security.Cryptography;
using System.Text;
using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// File-backed content-addressed cache. Payloads are stored under subfolders per
/// kind (markdown, images, metadata) keyed by a SHA-256 content hash.
/// </summary>
public class CacheService : ICacheService
{
    private readonly Config.CacheSettings _settings;
    private long _hits;
    private long _misses;

    public bool Enabled => _settings.Enabled;
    public string Directory { get; }
    public long Hits => Interlocked.Read(ref _hits);
    public long Misses => Interlocked.Read(ref _misses);

    public CacheService(GeneratorConfig config, string projectRoot)
    {
        _settings = config.Cache;
        Directory = Path.GetFullPath(Path.Combine(projectRoot, _settings.Directory));
    }

    public string ComputeHash(string content)
    {
        return ComputeHash(Encoding.UTF8.GetBytes(content));
    }

    public string ComputeHash(byte[] content)
    {
        var bytes = SHA256.HashData(content);
        return Convert.ToHexString(bytes)[..32].ToLowerInvariant();
    }

    public bool TryReadText(string key, string kind, out string text)
    {
        text = string.Empty;
        if (!Enabled)
        {
            return false;
        }

        var path = GetPath(key, kind, "txt");
        if (!File.Exists(path))
        {
            Interlocked.Increment(ref _misses);
            return false;
        }

        try
        {
            text = File.ReadAllText(path);
            Interlocked.Increment(ref _hits);
            return true;
        }
        catch
        {
            Interlocked.Increment(ref _misses);
            return false;
        }
    }

    public void StoreText(string key, string kind, string text)
    {
        if (!Enabled)
        {
            return;
        }

        var path = GetPath(key, kind, "txt");
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    public bool TryReadBytes(string key, string kind, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (!Enabled)
        {
            return false;
        }

        var path = GetPath(key, kind, "bin");
        if (!File.Exists(path))
        {
            Interlocked.Increment(ref _misses);
            return false;
        }

        try
        {
            bytes = File.ReadAllBytes(path);
            Interlocked.Increment(ref _hits);
            return true;
        }
        catch
        {
            Interlocked.Increment(ref _misses);
            return false;
        }
    }

    public void StoreBytes(string key, string kind, byte[] bytes)
    {
        if (!Enabled)
        {
            return;
        }

        var path = GetPath(key, kind, "bin");
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }

    private string GetPath(string key, string kind, string extension)
    {
        return Path.Combine(Directory, kind, $"{key}.{extension}");
    }
}
