namespace FiapX.Application.DTOs;

public record UploadVideoResponse
{
    public Guid VideoId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}