using FiapX.Application.UseCases.Videos;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FiapX.Application.Tests.UseCases.Videos;

public class GetUserVideosUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly GetUserVideosUseCase _sut;

    public GetUserVideosUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _videoRepositoryMock = new Mock<IVideoRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Videos).Returns(_videoRepositoryMock.Object);

        _sut = new GetUserVideosUseCase(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUserId_ShouldReturnUserVideos()
    {
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");

        var video1 = new Video(userId, "video1.mp4", "/storage/video1.mp4", 1024 * 1024);
        video1.MarkAsQueued();

        var video2 = new Video(userId, "video2.mp4", "/storage/video2.mp4", 2 * 1024 * 1024);
        video2.MarkAsQueued();
        video2.StartProcessing();

        var videos = new List<Video> { video1, video2 };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _videoRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videos);

        var result = await _sut.ExecuteAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        var videoList = result.Value.ToList();

        videoList[0].VideoId.Should().Be(video1.Id);
        videoList[0].OriginalFileName.Should().Be("video1.mp4");
        videoList[0].Status.Should().Be("Queued");
        videoList[0].FileSizeMB.Should().Be(1.0);

        videoList[1].VideoId.Should().Be(video2.Id);
        videoList[1].OriginalFileName.Should().Be("video2.mp4");
        videoList[1].Status.Should().Be("Processing");
        videoList[1].FileSizeMB.Should().Be(2.0);

        _videoRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ExecuteAsync(userId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Usuário não encontrado.");

        _videoRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithUserWithoutVideos_ShouldReturnEmptyList()
    {
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _videoRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Video>());

        var result = await _sut.ExecuteAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapStatusCorrectly()
    {
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");

        var videoUploaded = new Video(userId, "v1.mp4", "/storage/v1.mp4", 1024);

        var videoQueued = new Video(userId, "v2.mp4", "/storage/v2.mp4", 1024);
        videoQueued.MarkAsQueued();

        var videoProcessing = new Video(userId, "v3.mp4", "/storage/v3.mp4", 1024);
        videoProcessing.MarkAsQueued();
        videoProcessing.StartProcessing();

        var videoCompleted = new Video(userId, "v4.mp4", "/storage/v4.mp4", 1024);
        videoCompleted.MarkAsQueued();
        videoCompleted.StartProcessing();
        videoCompleted.CompleteProcessing("/storage/v4.zip", 100, TimeSpan.FromSeconds(10));

        var videoFailed = new Video(userId, "v5.mp4", "/storage/v5.mp4", 1024);
        videoFailed.MarkAsQueued();
        videoFailed.StartProcessing();
        videoFailed.FailProcessing("Error");

        var videos = new List<Video>
        {
            videoUploaded,
            videoQueued,
            videoProcessing,
            videoCompleted,
            videoFailed
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _videoRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videos);

        var result = await _sut.ExecuteAsync(userId);

        var videoList = result.Value.ToList();

        videoList[0].Status.Should().Be("Uploaded");
        videoList[0].StatusDescription.Should().Be("Enviado");

        videoList[1].Status.Should().Be("Queued");
        videoList[1].StatusDescription.Should().Be("Na fila");

        videoList[2].Status.Should().Be("Processing");
        videoList[2].StatusDescription.Should().Be("Processando");

        videoList[3].Status.Should().Be("Completed");
        videoList[3].StatusDescription.Should().Be("Concluído");
        videoList[3].CanDownload.Should().BeTrue();
        videoList[3].FrameCount.Should().Be(100);
        videoList[3].ProcessedAt.Should().NotBeNull();

        videoList[4].Status.Should().Be("Failed");
        videoList[4].StatusDescription.Should().Be("Falha");
        videoList[4].CanDownload.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateFileSizeInMBCorrectly()
    {
        var userId = Guid.NewGuid();
        var user = new User("user@test.com", "hash", "User");

        var video1 = new Video(userId, "small.mp4", "/storage/small.mp4", 512 * 1024); 
        var video2 = new Video(userId, "medium.mp4", "/storage/medium.mp4", 5 * 1024 * 1024);
        var video3 = new Video(userId, "large.mp4", "/storage/large.mp4", 100 * 1024 * 1024);

        var videos = new List<Video> { video1, video2, video3 };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _videoRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videos);

        var result = await _sut.ExecuteAsync(userId);

        var videoList = result.Value.ToList();

        videoList[0].FileSizeMB.Should().Be(0.5);
        videoList[1].FileSizeMB.Should().Be(5.0);
        videoList[2].FileSizeMB.Should().Be(100.0);
    }
}