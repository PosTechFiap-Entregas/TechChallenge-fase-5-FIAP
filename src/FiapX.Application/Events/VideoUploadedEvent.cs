namespace FiapX.Application.Events;

public record VideoUploadedEvent
{
    public Guid VideoId { get; init; }
    public Guid UserId { get; init; }
    public string StoragePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTime UploadedAt { get; init; }
}