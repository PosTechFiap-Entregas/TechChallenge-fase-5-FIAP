using FiapX.Domain.Interfaces;
using FiapX.Shared.Constants;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace FiapX.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de processamento de vídeo usando Xabe.FFmpeg.
/// Extrai frames do vídeo e compacta em arquivo ZIP.
/// Baixa FFmpeg automaticamente se necessário.
/// </summary>
public class FFmpegVideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<FFmpegVideoProcessingService> _logger;
    private static bool _ffmpegDownloaded = false;
    private static readonly SemaphoreSlim _downloadLock = new(1, 1);

    public FFmpegVideoProcessingService(ILogger<FFmpegVideoProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(
        string videoPath,
        string outputDirectory,
        int fps = 1,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Garantir que FFmpeg está disponível
            await EnsureFFmpegAsync();

            _logger.LogInformation("Iniciando processamento do vídeo: {VideoPath}", videoPath);

            // Validações
            if (!File.Exists(videoPath))
            {
                return new VideoProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Arquivo de vídeo não encontrado."
                };
            }

            // Criar diretório de frames temporário
            var framesDirectory = Path.Combine(outputDirectory, "frames");
            Directory.CreateDirectory(framesDirectory);

            // Extrair frames usando Xabe.FFmpeg
            var frameCount = await ExtractFramesAsync(videoPath, framesDirectory, fps, cancellationToken);

            if (frameCount == 0)
            {
                return new VideoProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Nenhum frame foi extraído do vídeo."
                };
            }

            _logger.LogInformation("{FrameCount} frames extraídos com sucesso", frameCount);

            // Criar arquivo ZIP com os frames
            var zipFileName = string.Format(VideoConstants.ZipFilePattern, DateTime.UtcNow);
            var zipPath = Path.Combine(outputDirectory, zipFileName);

            await CreateZipFromFramesAsync(framesDirectory, zipPath, cancellationToken);

            _logger.LogInformation("ZIP criado com sucesso: {ZipPath}", zipPath);

            // Limpar diretório de frames temporário
            Directory.Delete(framesDirectory, recursive: true);

            stopwatch.Stop();

            return new VideoProcessingResult
            {
                Success = true,
                ZipPath = zipPath,
                FrameCount = frameCount,
                ProcessingDuration = stopwatch.Elapsed
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Processamento cancelado para: {VideoPath}", videoPath);
            stopwatch.Stop();

            return new VideoProcessingResult
            {
                Success = false,
                ErrorMessage = "Processamento cancelado.",
                ProcessingDuration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar vídeo: {VideoPath}", videoPath);
            stopwatch.Stop();

            return new VideoProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingDuration = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Garante que o FFmpeg está disponível, baixando se necessário
    /// </summary>
    private async Task EnsureFFmpegAsync()
    {
        if (_ffmpegDownloaded)
            return;

        await _downloadLock.WaitAsync();
        try
        {
            if (_ffmpegDownloaded)
                return;

            // Definir diretório onde FFmpeg será baixado
            var ffmpegPath = Path.Combine(Path.GetTempPath(), "ffmpeg");
            Directory.CreateDirectory(ffmpegPath);

            _logger.LogInformation("Verificando FFmpeg em: {Path}", ffmpegPath);

            // Verificar se já existe
            var ffmpegExecutable = Path.Combine(ffmpegPath, OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");

            if (!File.Exists(ffmpegExecutable))
            {
                _logger.LogInformation("FFmpeg não encontrado. Iniciando download...");

                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);

                _logger.LogInformation("FFmpeg baixado com sucesso!");
            }
            else
            {
                _logger.LogInformation("FFmpeg já está disponível");
            }

            // Configurar Xabe.FFmpeg para usar esse diretório
            FFmpeg.SetExecutablesPath(ffmpegPath);

            _ffmpegDownloaded = true;
        }
        finally
        {
            _downloadLock.Release();
        }
    }

    /// <summary>
    /// Extrai frames do vídeo usando Xabe.FFmpeg
    /// </summary>
    private async Task<int> ExtractFramesAsync(
        string videoPath,
        string outputDirectory,
        int fps,
        CancellationToken cancellationToken)
    {
        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath, cancellationToken);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

            if (videoStream == null)
                throw new InvalidOperationException("Nenhum stream de vídeo encontrado");

            var framePattern = Path.Combine(outputDirectory, "frame_%04d.png");

            // Configurar conversão para extrair frames
            var conversion = FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .SetFrameRate(fps)
                .SetOutput(framePattern);

            await conversion.Start(cancellationToken);

            // Contar frames extraídos
            var frameFiles = Directory.GetFiles(outputDirectory, "*.png");
            return frameFiles.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair frames do vídeo");
            throw new InvalidOperationException($"Falha ao extrair frames: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cria arquivo ZIP a partir dos frames extraídos
    /// </summary>
    private async Task CreateZipFromFramesAsync(
        string framesDirectory,
        string zipPath,
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            ZipFile.CreateFromDirectory(
                framesDirectory,
                zipPath,
                CompressionLevel.Optimal,
                includeBaseDirectory: false);

        }, cancellationToken);
    }
}