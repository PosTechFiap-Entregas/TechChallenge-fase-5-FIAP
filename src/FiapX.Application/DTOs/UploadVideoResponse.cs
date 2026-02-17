using FiapX.Domain.Enums;

namespace FiapX.Application.DTOs;

/// <summary>
/// DTO de resposta do upload
/// </summary>
public record UploadVideoResponse
{
    public Guid VideoId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}