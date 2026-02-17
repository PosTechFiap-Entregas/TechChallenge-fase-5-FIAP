namespace FiapX.Shared.Constants;

/// <summary>
/// Constantes relacionadas a vídeos
/// </summary>
public static class VideoConstants
{
    public const long MaxFileSizeBytes = 2L * 1024 * 1024 * 1024; // 2GB
    public const int MaxFileSizeMB = 2048;

    public static readonly string[] AllowedExtensions = 
    {
        ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv", ".webm", ".m4v"
    };

    public static readonly string[] AllowedMimeTypes = 
    {
        "video/mp4",
        "video/x-msvideo",
        "video/quicktime",
        "video/x-matroska",
        "video/x-ms-wmv",
        "video/x-flv",
        "video/webm"
    };

    public const int DefaultFps = 1; // 1 frame por segundo
    public const string FrameFilePattern = "frame_{0:D4}.png";
    public const string ZipFilePattern = "frames_{0:yyyyMMdd_HHmmss}.zip";
}

/// <summary>
/// Constantes de autenticação e autorização
/// </summary>
public static class AuthConstants
{
    public const int MinPasswordLength = 6;
    public const int MaxPasswordLength = 100;
    public const int MinNameLength = 2;
    public const int MaxNameLength = 200;
    public const int TokenExpirationMinutes = 60;
}

/// <summary>
/// Constantes de mensagens
/// </summary>
public static class MessageConstants
{
    public const string VideoUploadedQueue = "video-processing-queue";
    public const string VideoProcessedQueue = "video-processed-queue";
    public const string VideoUploadedExchange = "video-processing-exchange";
}

/// <summary>
/// Constantes de cache
/// </summary>
public static class CacheConstants
{
    public const string VideoStatusKeyPattern = "video:status:{0}";
    public const string UserVideosKeyPattern = "user:videos:{0}";
    public const int VideoStatusTtlMinutes = 5;
    public const int UserVideosTtlMinutes = 2;
}
