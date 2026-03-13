using Prometheus;

namespace FiapX.Worker.Services;

public class VideoMetricsService
{
    private static readonly Counter VideosProcessedTotal = Metrics.CreateCounter(
        "fiapx_video_processed_total",
        "Total de vídeos processados",
        new CounterConfiguration
        {
            LabelNames = new[] { "status" }
        });

    private static readonly Counter FramesExtractedTotal = Metrics.CreateCounter(
        "fiapx_frames_extracted_total",
        "Total de frames extraídos de todos os vídeos");

    private static readonly Histogram ProcessingDuration = Metrics.CreateHistogram(
        "fiapx_video_processing_duration_seconds",
        "Tempo de processamento de vídeos em segundos",
        new HistogramConfiguration
        {
            Buckets = new double[] { 1, 5, 10, 30, 60, 120, 300 }
        });

    private static readonly Histogram FramesPerVideo = Metrics.CreateHistogram(
        "fiapx_frames_per_video",
        "Número de frames extraídos por vídeo",
        new HistogramConfiguration
        {
            Buckets = new double[] { 10, 50, 100, 300, 600, 1200, 3600 }
        });

    private static readonly Gauge VideosInProgress = Metrics.CreateGauge(
        "fiapx_videos_in_progress",
        "Número de vídeos sendo processados neste momento");

    public void RecordVideoProcessingStarted()
    {
        VideosInProgress.Inc();
    }

    public void RecordVideoProcessingSuccess(double durationSeconds, int frameCount)
    {
        VideosInProgress.Dec();
        VideosProcessedTotal.WithLabels("success").Inc();
        FramesExtractedTotal.Inc(frameCount);
        ProcessingDuration.Observe(durationSeconds);
        FramesPerVideo.Observe(frameCount);
    }

    public void RecordVideoProcessingFailed(double durationSeconds)
    {
        VideosInProgress.Dec();
        VideosProcessedTotal.WithLabels("failed").Inc();
        ProcessingDuration.Observe(durationSeconds);
    }
}