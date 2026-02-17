using FiapX.Domain.Enums;

namespace FiapX.Application.DTOs;

/// <summary>
/// DTO de listagem de vídeos do usuário
/// </summary>
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