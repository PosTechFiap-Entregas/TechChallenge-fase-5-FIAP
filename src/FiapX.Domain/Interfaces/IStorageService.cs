namespace FiapX.Domain.Interfaces;

/// <summary>
/// Interface para serviço de armazenamento de arquivos.
/// Definida no Domain para que Application possa usar sem referenciar Infrastructure.
/// </summary>
public interface IStorageService
{
    Task<string> SaveVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> SaveZipAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);
    string GetTempDirectory();
    Task CleanupTempFilesAsync(CancellationToken cancellationToken = default);
}