namespace FiapX.Application.Events;

/// <summary>
/// Evento publicado quando um vídeo é enviado com sucesso.
/// Vive no Application porque é um contrato de comunicação entre use cases e workers.
/// </summary>
public record VideoUploadedEvent
{
    public Guid VideoId { get; init; }
    public Guid UserId { get; init; }
    public string StoragePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTime UploadedAt { get; init; }
}