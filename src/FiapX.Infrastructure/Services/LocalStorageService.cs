using FiapX.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FiapX.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de armazenamento usando sistema de arquivos local
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _storageBasePath;
    private readonly string _uploadsPath;
    private readonly string _outputsPath;
    private readonly string _tempPath;

    public LocalStorageService(IConfiguration configuration)
    {
        var basePath = configuration["Storage:BasePath"] ?? "./storage";

        // Converter para caminho absoluto
        _storageBasePath = Path.IsPathRooted(basePath)
            ? basePath
            : Path.GetFullPath(basePath);

        _uploadsPath = Path.Combine(_storageBasePath, "uploads");
        _outputsPath = Path.Combine(_storageBasePath, "outputs");
        _tempPath = Path.Combine(_storageBasePath, "temp");

        EnsureDirectoriesExist();
    }

    public async Task<string> SaveVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var safeFileName = $"{timestamp}_{SanitizeFileName(fileName)}";
        var fullPath = Path.Combine(_uploadsPath, safeFileName);

        await using var fileStreamOutput = File.Create(fullPath);
        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        return fullPath;
    }

    public async Task<string> SaveZipAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var safeFileName = SanitizeFileName(fileName);
        var fullPath = Path.Combine(_outputsPath, safeFileName);

        await using var fileStreamOutput = File.Create(fullPath);
        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        return fullPath;
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        Stream stream = File.OpenRead(filePath);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var fileInfo = new FileInfo(filePath);
        return Task.FromResult(fileInfo.Length);
    }

    public string GetTempDirectory()
    {
        var uniqueId = Guid.NewGuid().ToString("N");
        var tempDir = Path.Combine(_tempPath, uniqueId);
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    public Task CleanupTempFilesAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_tempPath))
            return Task.CompletedTask;

        var directories = Directory.GetDirectories(_tempPath);
        var cutoffTime = DateTime.UtcNow.AddHours(-2); // Remove arquivos com mais de 2 horas

        foreach (var directory in directories)
        {
            var dirInfo = new DirectoryInfo(directory);
            if (dirInfo.CreationTimeUtc < cutoffTime)
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                    // Ignora erros de limpeza
                }
            }
        }

        return Task.CompletedTask;
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_uploadsPath);
        Directory.CreateDirectory(_outputsPath);
        Directory.CreateDirectory(_tempPath);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}