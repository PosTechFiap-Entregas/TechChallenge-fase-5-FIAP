using Prometheus;

namespace FiapX.Worker.Services;

/// <summary>
/// Serviço para coletar métricas customizadas do processamento de vídeos
/// </summary>
public class VideoMetricsService
{
    // ═══════════════════════════════════════════════════════════════════════
    // COUNTERS - Contadores que sempre sobem
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Total de vídeos processados (sucesso + falha)
    /// </summary>
    private static readonly Counter VideosProcessedTotal = Metrics.CreateCounter(
        "fiapx_video_processed_total",
        "Total de vídeos processados",
        new CounterConfiguration
        {
            LabelNames = new[] { "status" } // success, failed
        });

    /// <summary>
    /// Total de frames extraídos
    /// </summary>
    private static readonly Counter FramesExtractedTotal = Metrics.CreateCounter(
        "fiapx_frames_extracted_total",
        "Total de frames extraídos de todos os vídeos");

    // ═══════════════════════════════════════════════════════════════════════
    // HISTOGRAMS - Distribuição de valores
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Duração do processamento de vídeos (em segundos)
    /// </summary>
    private static readonly Histogram ProcessingDuration = Metrics.CreateHistogram(
        "fiapx_video_processing_duration_seconds",
        "Tempo de processamento de vídeos em segundos",
        new HistogramConfiguration
        {
            Buckets = new double[] { 1, 5, 10, 30, 60, 120, 300 } // 1s, 5s, 10s, 30s, 1min, 2min, 5min
        });

    /// <summary>
    /// Número de frames por vídeo
    /// </summary>
    private static readonly Histogram FramesPerVideo = Metrics.CreateHistogram(
        "fiapx_frames_per_video",
        "Número de frames extraídos por vídeo",
        new HistogramConfiguration
        {
            Buckets = new double[] { 10, 50, 100, 300, 600, 1200, 3600 } // 10s até 1h de vídeo
        });

    // ═══════════════════════════════════════════════════════════════════════
    // GAUGES - Valores que sobem e descem
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Vídeos sendo processados neste momento
    /// </summary>
    private static readonly Gauge VideosInProgress = Metrics.CreateGauge(
        "fiapx_videos_in_progress",
        "Número de vídeos sendo processados neste momento");

    // ═══════════════════════════════════════════════════════════════════════
    // MÉTODOS PÚBLICOS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Incrementa contador quando processamento inicia
    /// </summary>
    public void RecordVideoProcessingStarted()
    {
        VideosInProgress.Inc();
    }

    /// <summary>
    /// Registra sucesso no processamento
    /// </summary>
    /// <param name="durationSeconds">Duração do processamento em segundos</param>
    /// <param name="frameCount">Número de frames extraídos</param>
    public void RecordVideoProcessingSuccess(double durationSeconds, int frameCount)
    {
        VideosInProgress.Dec();
        VideosProcessedTotal.WithLabels("success").Inc();
        FramesExtractedTotal.Inc(frameCount);
        ProcessingDuration.Observe(durationSeconds);
        FramesPerVideo.Observe(frameCount);
    }

    /// <summary>
    /// Registra falha no processamento
    /// </summary>
    /// <param name="durationSeconds">Duração até a falha em segundos</param>
    public void RecordVideoProcessingFailed(double durationSeconds)
    {
        VideosInProgress.Dec();
        VideosProcessedTotal.WithLabels("failed").Inc();
        ProcessingDuration.Observe(durationSeconds);
    }
}