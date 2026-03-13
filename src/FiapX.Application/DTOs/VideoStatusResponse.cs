namespace FiapX.Application.DTOs;

public record VideoStatusResponse
{
    public Guid VideoId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusDescription { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public int? FrameCount { get; init; }
    public string? ErrorMessage { get; init; }
    public double? ProcessingDurationSeconds { get; init; }
    public bool CanDownload { get; init; }
}