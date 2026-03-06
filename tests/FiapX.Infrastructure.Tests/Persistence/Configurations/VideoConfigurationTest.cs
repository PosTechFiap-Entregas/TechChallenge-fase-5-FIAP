using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Persistence.Configurations;

public class VideoConfigurationTests : IDisposable
{
    private readonly AppDbContext _context;

    public VideoConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public void VideoConfiguration_ShouldMapToTable_Videos()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Videos");
    }

    [Fact]
    public void VideoConfiguration_ShouldHavePrimaryKey_Id()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var pk = entityType.FindPrimaryKey();

        pk.Should().NotBeNull();
        pk!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void VideoConfiguration_Id_ShouldBeValueGeneratedNever()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("Id")!;

        property.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public void VideoConfiguration_UserId_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("UserId")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void VideoConfiguration_OriginalFileName_ShouldBeRequired_WithMaxLength255()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("OriginalFileName")!;

        property.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void VideoConfiguration_StoragePath_ShouldBeRequired_WithMaxLength500()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("StoragePath")!;

        property.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void VideoConfiguration_FileSizeBytes_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("FileSizeBytes")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void VideoConfiguration_Status_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("Status")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void VideoConfiguration_UploadedAt_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("UploadedAt")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void VideoConfiguration_CreatedAt_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("CreatedAt")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void VideoConfiguration_ProcessedAt_ShouldBeNullable()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("ProcessedAt")!;

        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void VideoConfiguration_ZipPath_ShouldBeNullable_WithMaxLength500()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("ZipPath")!;

        property.IsNullable.Should().BeTrue();
        property.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void VideoConfiguration_FrameCount_ShouldBeNullable()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("FrameCount")!;

        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void VideoConfiguration_ErrorMessage_ShouldBeNullable()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("ErrorMessage")!;

        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void VideoConfiguration_ProcessingDuration_ShouldBeNullable()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("ProcessingDuration")!;

        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void VideoConfiguration_UpdatedAt_ShouldBeNullable()
    {
        var property = _context.Model.FindEntityType(typeof(Video))!.FindProperty("UpdatedAt")!;

        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void VideoConfiguration_UserId_ShouldHaveIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.Any(p => p.Name == "UserId"));

        index.Should().NotBeNull();
    }

    [Fact]
    public void VideoConfiguration_Status_ShouldHaveIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties.Any(p => p.Name == "Status"));

        index.Should().NotBeNull();
    }

    [Fact]
    public void VideoConfiguration_UploadedAt_ShouldHaveIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "UploadedAt"));

        index.Should().NotBeNull();
    }

    [Fact]
    public void VideoConfiguration_ProcessedAt_ShouldHaveIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "ProcessedAt"));

        index.Should().NotBeNull();
    }

    [Fact]
    public void VideoConfiguration_ShouldHaveCompositeIndex_UserIdAndStatus()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var compositeIndex = entityType.GetIndexes()
            .FirstOrDefault(i =>
                i.Properties.Count == 2 &&
                i.Properties.Any(p => p.Name == "UserId") &&
                i.Properties.Any(p => p.Name == "Status"));

        compositeIndex.Should().NotBeNull("deve existir índice composto em UserId + Status");
    }

    [Fact]
    public void VideoConfiguration_ShouldHaveManyToOneRelationship_WithUser()
    {
        var entityType = _context.Model.FindEntityType(typeof(Video))!;
        var navigation = entityType.GetNavigations()
            .FirstOrDefault(n => n.Name == "User");

        navigation.Should().NotBeNull();
    }

    private User CreateAndSaveUser(string email = "owner@example.com")
    {
        var user = new User(email, "hash", "Owner");
        _context.Users.Add(user);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return user;
    }

    [Fact]
    public async Task VideoConfiguration_ShouldPersistAndRetrieveVideo()
    {
        var user = CreateAndSaveUser();
        var video = new Video(user.Id, "video.mp4", "/storage/video.mp4", 1024);

        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _context.Videos.FindAsync(video.Id);

        retrieved.Should().NotBeNull();
        retrieved!.OriginalFileName.Should().Be("video.mp4");
        retrieved.FileSizeBytes.Should().Be(1024);
        retrieved.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task VideoConfiguration_Status_ShouldDefaultToUploaded()
    {
        var user = CreateAndSaveUser("status@example.com");
        var video = new Video(user.Id, "clip.mp4", "/path/clip.mp4", 512);

        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _context.Videos.FindAsync(video.Id);

        retrieved!.Status.Should().Be(VideoStatus.Uploaded);
    }

    [Fact]
    public async Task VideoConfiguration_NullableFields_ShouldPersistAsNull()
    {
        var user = CreateAndSaveUser("nullable@example.com");
        var video = new Video(user.Id, "file.mp4", "/path/file.mp4", 256);

        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _context.Videos.FindAsync(video.Id);

        retrieved!.ProcessedAt.Should().BeNull();
        retrieved.ZipPath.Should().BeNull();
        retrieved.FrameCount.Should().BeNull();
        retrieved.ErrorMessage.Should().BeNull();
        retrieved.ProcessingDuration.Should().BeNull();
    }
}
