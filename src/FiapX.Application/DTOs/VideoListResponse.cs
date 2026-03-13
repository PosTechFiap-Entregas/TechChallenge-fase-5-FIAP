namespace FiapX.Application.DTOs;

public record VideoListResponse
{
    public Guid VideoId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public double FileSizeMB { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusDescription { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public int? FrameCount { get; init; }
    public bool CanDownload { get; init; }
}