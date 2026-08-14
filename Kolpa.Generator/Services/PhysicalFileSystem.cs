using System.Text;
using Kolpa.Generator.Interfaces;

namespace Kolpa.Generator.Services;

/// <summary>
/// Physical implementation of IFileSystem using native System.IO API calls.
/// </summary>
public class PhysicalFileSystem : IFileSystem
{
    public async Task<string> ReadFileAsync(string path)
    {
        return await RetryOnIOError(() => File.ReadAllTextAsync(path, Encoding.UTF8));
    }

    public async Task<byte[]> ReadFileBytesAsync(string path)
    {
        return await RetryOnIOError(() => File.ReadAllBytesAsync(path));
    }

    private static async Task<T> RetryOnIOError<T>(
        Func<Task<T>> action,
        int maxRetries = 5,
        int baseDelayMs = 100
    )
    {
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (IOException) when (attempt < maxRetries)
            {
                await Task.Delay(baseDelayMs * (attempt + 1));
            }
        }
    }

    private static async Task RetryOnIOError(
        Func<Task> action,
        int maxRetries = 5,
        int baseDelayMs = 100
    )
    {
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (IOException) when (attempt < maxRetries)
            {
                await Task.Delay(baseDelayMs * (attempt + 1));
            }
        }
    }

    public async Task WriteFileAsync(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        await RetryOnIOError(() => File.WriteAllTextAsync(path, content, Encoding.UTF8));
    }

    public async Task WriteFileBytesAsync(string path, byte[] content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        await RetryOnIOError(() => File.WriteAllBytesAsync(path, content));
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive);
                }
                return;
            }
            catch (IOException) when (attempt < 5)
            {
                Thread.Sleep(100 * (attempt + 1));
            }
        }
    }

    public IEnumerable<string> EnumerateFiles(
        string path,
        string searchPattern,
        SearchOption searchOption
    )
    {
        if (!Directory.Exists(path))
        {
            return [];
        }
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    public void CopyFile(string source, string destination, bool overwrite)
    {
        var dir = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.Copy(source, destination, overwrite);
    }
}
