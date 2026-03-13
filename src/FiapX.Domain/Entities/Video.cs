using FiapX.Domain.Enums;

namespace FiapX.Domain.Entities;

public class Video : BaseEntity
{
    public Guid UserId { get; private set; }
    public string OriginalFileName { get; private set; }
    public string StoragePath { get; private set; }
    public long FileSizeBytes { get; private set; }
    public VideoStatus Status { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ZipPath { get; private set; }
    public int? FrameCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public TimeSpan? ProcessingDuration { get; private set; }

    public virtual User User { get; private set; } = null!;

    private Video() 
    { 
        OriginalFileName = string.Empty;
        StoragePath = string.Empty;
    }

    public Video(
        Guid userId,
        string originalFileName,
        string storagePath,
        long fileSizeBytes) : base()
    {
        ValidateFileName(originalFileName);
        ValidateStoragePath(storagePath);
        ValidateFileSize(fileSizeBytes);

        UserId = userId;
        OriginalFileName = originalFileName;
        StoragePath = storagePath;
        FileSizeBytes = fileSizeBytes;
        Status = VideoStatus.Uploaded;
        UploadedAt = DateTime.UtcNow;
    }

    public void MarkAsQueued()
    {
        if (Status != VideoStatus.Uploaded)
            throw new InvalidOperationException($"Cannot queue video in status {Status}");

        Status = VideoStatus.Queued;
        MarkAsUpdated();
    }

    public void StartProcessing()
    {
        if (Status != VideoStatus.Queued)
            throw new InvalidOperationException($"Cannot start processing video in status {Status}");

        Status = VideoStatus.Processing;
        MarkAsUpdated();
    }

    public void CompleteProcessing(string zipPath, int frameCount, TimeSpan duration)
    {
        if (Status != VideoStatus.Processing)
            throw new InvalidOperationException($"Cannot complete video not in processing status");

        ValidateZipPath(zipPath);

        Status = VideoStatus.Completed;
        ZipPath = zipPath;
        FrameCount = frameCount;
        ProcessedAt = DateTime.UtcNow;
        ProcessingDuration = duration;
        ErrorMessage = null;
        MarkAsUpdated();
    }

    public void FailProcessing(string errorMessage)
    {
        if (Status != VideoStatus.Processing)
            throw new InvalidOperationException($"Cannot fail video not in processing status");

        Status = VideoStatus.Failed;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void RetryProcessing()
    {
        if (Status != VideoStatus.Failed)
            throw new InvalidOperationException($"Cannot retry video not in failed status");

        Status = VideoStatus.Queued;
        ErrorMessage = null;
        ProcessedAt = null;
        FrameCount = null;
        ZipPath = null;
        ProcessingDuration = null;
        MarkAsUpdated();
    }

    public bool IsProcessed() => Status == VideoStatus.Completed;
    public bool IsFailed() => Status == VideoStatus.Failed;
    public bool IsProcessing() => Status == VideoStatus.Processing;
    public bool CanDownload() => Status == VideoStatus.Completed && !string.IsNullOrEmpty(ZipPath);

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (fileName.Length > 255)
            throw new ArgumentException("File name too long", nameof(fileName));
    }

    private static void ValidateStoragePath(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Storage path cannot be empty", nameof(storagePath));
    }

    private static void ValidateFileSize(long fileSize)
    {
        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));

        const long maxSize = 2L * 1024 * 1024 * 1024;
        if (fileSize > maxSize)
            throw new ArgumentException($"File size exceeds maximum of 2GB", nameof(fileSize));
    }

    private static void ValidateZipPath(string zipPath)
    {
        if (string.IsNullOrWhiteSpace(zipPath))
            throw new ArgumentException("Zip path cannot be empty", nameof(zipPath));
    }
}