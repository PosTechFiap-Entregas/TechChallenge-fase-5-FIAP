namespace FiapX.Application.DTOs;

public record VideoDownloadResponse
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
}