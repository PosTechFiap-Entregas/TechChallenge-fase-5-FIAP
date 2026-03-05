using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Infrastructure.Persistence.Context;
using FiapX.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Repositories;

public class VideoRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VideoRepository _repository;
    private readonly Guid _userId;

    public VideoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new VideoRepository(_context);

        _userId = Guid.NewGuid();
        var user = new User("test@example.com", "hash", "Test User");
        typeof(User).GetProperty("Id")!.SetValue(user, _userId);
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnVideosOrderedByUploadedAtDescending()
    {
        var video1 = new Video(_userId, "video1.mp4", "/path1", 1024);
        var video2 = new Video(_userId, "video2.mp4", "/path2", 2048);
        var video3 = new Video(_userId, "video3.mp4", "/path3", 3072);

        await _context.Videos.AddRangeAsync(video1, video2, video3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(_userId);

        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        resultList[0].OriginalFileName.Should().Be("video3.mp4");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistentUserId_ShouldReturnEmpty()
    {
        var result = await _repository.GetByUserIdAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnVideosWithSpecificStatus()
    {
        var video1 = new Video(_userId, "queued.mp4", "/path1", 1024);
        video1.MarkAsQueued();

        var video2 = new Video(_userId, "uploaded.mp4", "/path2", 2048);

        var video3 = new Video(_userId, "queued2.mp4", "/path3", 3072);
        video3.MarkAsQueued();

        await _context.Videos.AddRangeAsync(video1, video2, video3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByStatusAsync(VideoStatus.Queued);

        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().AllSatisfy(v => v.Status.Should().Be(VideoStatus.Queued));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOrderedByUploadedAt()
    {
        var video1 = new Video(_userId, "video1.mp4", "/path1", 1024);
        var video2 = new Video(_userId, "video2.mp4", "/path2", 2048);

        await _context.Videos.AddRangeAsync(video1, video2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByStatusAsync(VideoStatus.Uploaded);

        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].OriginalFileName.Should().Be("video1.mp4");
    }

    [Fact]
    public async Task GetPendingVideosAsync_ShouldReturnQueuedAndProcessingVideos()
    {
        var video1 = new Video(_userId, "uploaded.mp4", "/path1", 1024);

        var video2 = new Video(_userId, "queued.mp4", "/path2", 2048);
        video2.MarkAsQueued();

        var video3 = new Video(_userId, "processing.mp4", "/path3", 3072);
        video3.MarkAsQueued();
        video3.StartProcessing();

        var video4 = new Video(_userId, "completed.mp4", "/path4", 4096);
        video4.MarkAsQueued();
        video4.StartProcessing();
        video4.CompleteProcessing("/zip", 100, TimeSpan.FromSeconds(10));

        await _context.Videos.AddRangeAsync(video1, video2, video3, video4);
        await _context.SaveChangesAsync();

        var result = await _repository.GetPendingVideosAsync();

        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(v => v.Status == VideoStatus.Queued);
        resultList.Should().Contain(v => v.Status == VideoStatus.Processing);
    }

    [Fact]
    public async Task GetByIdWithUserAsync_ShouldReturnVideoWithUser()
    {
        var video = new Video(_userId, "video.mp4", "/path", 1024);
        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdWithUserAsync(video.Id);

        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdWithUserAsync_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _repository.GetByIdWithUserAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CountByUserIdAsync_ShouldReturnCorrectCount()
    {
        var video1 = new Video(_userId, "video1.mp4", "/path1", 1024);
        var video2 = new Video(_userId, "video2.mp4", "/path2", 2048);
        var video3 = new Video(_userId, "video3.mp4", "/path3", 3072);

        await _context.Videos.AddRangeAsync(video1, video2, video3);
        await _context.SaveChangesAsync();

        var count = await _repository.CountByUserIdAsync(_userId);

        count.Should().Be(3);
    }

    [Fact]
    public async Task CountByUserIdAsync_WithNonExistentUserId_ShouldReturnZero()
    {
        var count = await _repository.CountByUserIdAsync(Guid.NewGuid());

        count.Should().Be(0);
    }

    [Fact]
    public async Task CountByStatusAsync_ShouldReturnCorrectCount()
    {
        var video1 = new Video(_userId, "video1.mp4", "/path1", 1024);
        video1.MarkAsQueued();

        var video2 = new Video(_userId, "video2.mp4", "/path2", 2048);
        video2.MarkAsQueued();

        var video3 = new Video(_userId, "video3.mp4", "/path3", 3072);

        await _context.Videos.AddRangeAsync(video1, video2, video3);
        await _context.SaveChangesAsync();

        var count = await _repository.CountByStatusAsync(VideoStatus.Queued);

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnVideo()
    {
        var video = new Video(_userId, "video.mp4", "/path", 1024);
        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(video.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(video.Id);
        result.OriginalFileName.Should().Be("video.mp4");
    }

    [Fact]
    public async Task AddAsync_ShouldAddVideoToDatabase()
    {
        var video = new Video(_userId, "new-video.mp4", "/path", 1024);

        await _repository.AddAsync(video);
        await _context.SaveChangesAsync();

        var savedVideo = await _context.Videos.FindAsync(video.Id);
        savedVideo.Should().NotBeNull();
        savedVideo!.OriginalFileName.Should().Be("new-video.mp4");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}