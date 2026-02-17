namespace FiapX.Domain.Interfaces;

/// <summary>
/// Interface para processamento de vídeos (extração de frames).
/// Definida no Domain para que Application e Workers possam usar sem referenciar Infrastructure.
/// </summary>
public interface IVideoProcessingService
{
    /// <summary>
    /// Processa um vídeo extraindo frames e gerando arquivo ZIP
    /// </summary>
    /// <param name="videoPath">Caminho completo do vídeo no storage</param>
    /// <param name="outputDirectory">Diretório onde os frames e ZIP serão salvos</param>
    /// <param name="fps">Frames por segundo a serem extraídos (padrão: 1)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado com caminho do ZIP e quantidade de frames extraídos</returns>
    Task<VideoProcessingResult> ProcessVideoAsync(
        string videoPath,
        string outputDirectory,
        int fps = 1,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado do processamento de vídeo
/// </summary>
public record VideoProcessingResult
{
    public bool Success { get; init; }
    public string? ZipPath { get; init; }
    public int FrameCount { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ProcessingDuration { get; init; }
}