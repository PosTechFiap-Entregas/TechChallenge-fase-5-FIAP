namespace FiapX.Domain.Interfaces;

public interface IVideoProcessingService
{
    Task<VideoProcessingResult> ProcessVideoAsync(
        string videoPath,
        string outputDirectory,
        int fps = 1,
        CancellationToken cancellationToken = default);
}

public record VideoProcessingResult
{
    public bool Success { get; init; }
    public string? ZipPath { get; init; }
    public int FrameCount { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ProcessingDuration { get; init; }
}