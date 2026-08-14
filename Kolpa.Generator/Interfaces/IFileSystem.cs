namespace Kolpa.Generator.Interfaces;

/// <summary>
/// Abstraction layer for file system access, decoupling business logic from System.IO.
/// </summary>
public interface IFileSystem
{
    Task<string> ReadFileAsync(string path);
    Task<byte[]> ReadFileBytesAsync(string path);
    Task WriteFileAsync(string path, string content);
    Task WriteFileBytesAsync(string path, byte[] content);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    void DeleteDirectory(string path, bool recursive);
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    void CopyFile(string source, string destination, bool overwrite);
}
