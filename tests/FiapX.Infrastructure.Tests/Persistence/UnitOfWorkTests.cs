using FiapX.Domain.Entities;
using FiapX.Infrastructure.Persistence;
using FiapX.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public void Users_ShouldReturnUserRepository()
    {
        var usersRepo = _unitOfWork.Users;

        usersRepo.Should().NotBeNull();
        usersRepo.Should().BeAssignableTo<Domain.Interfaces.IUserRepository>();
    }

    [Fact]
    public void Videos_ShouldReturnVideoRepository()
    {
        var videosRepo = _unitOfWork.Videos;

        videosRepo.Should().NotBeNull();
        videosRepo.Should().BeAssignableTo<Domain.Interfaces.IVideoRepository>();
    }

    [Fact]
    public void Users_ShouldReturnSameInstanceOnMultipleCalls()
    {
        var repo1 = _unitOfWork.Users;
        var repo2 = _unitOfWork.Users;

        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void Videos_ShouldReturnSameInstanceOnMultipleCalls()
    {
        var repo1 = _unitOfWork.Videos;
        var repo2 = _unitOfWork.Videos;

        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        var user = new User("test@example.com", "hash", "Test User");
        await _unitOfWork.Users.AddAsync(user);

        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().Be(1);
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldNotThrowException()
    {
        var act = async () => await _unitOfWork.BeginTransactionAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CommitAsync_ShouldSaveChangesWithoutTransaction()
    {
        var user = new User("test@example.com", "hash", "Test User");
        await _unitOfWork.Users.AddAsync(user);

        await _unitOfWork.CommitAsync();

        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task RollbackAsync_ShouldNotThrowException()
    {
        var act = async () => await _unitOfWork.RollbackAsync();
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}