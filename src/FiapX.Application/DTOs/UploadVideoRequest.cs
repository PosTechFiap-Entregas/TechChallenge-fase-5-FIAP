namespace FiapX.Application.DTOs;

/// <summary>
/// DTO para upload de vídeo
/// </summary>
public record UploadVideoRequest
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public Guid UserId { get; init; }
}