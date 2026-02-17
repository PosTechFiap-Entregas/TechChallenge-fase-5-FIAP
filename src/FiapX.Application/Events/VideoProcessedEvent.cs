namespace FiapX.Application.Events;

/// <summary>
/// Evento publicado quando um vídeo é processado com sucesso ou falha.
/// Vive no Application porque é um contrato de comunicação entre workers e outros consumidores.
/// </summary>
public record VideoProcessedEvent
{
    public Guid VideoId { get; init; }
    public Guid UserId { get; init; }
    public bool Success { get; init; }
    public int? FrameCount { get; init; }
    public string? ZipPath { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan? ProcessingDuration { get; init; }
    public DateTime ProcessedAt { get; init; }
}