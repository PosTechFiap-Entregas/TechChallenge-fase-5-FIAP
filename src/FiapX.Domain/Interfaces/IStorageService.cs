namespace FiapX.Domain.Interfaces;

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