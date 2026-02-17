namespace FiapX.Application.DTOs;

/// <summary>
/// DTO para download do ZIP
/// </summary>
public record VideoDownloadResponse
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
}