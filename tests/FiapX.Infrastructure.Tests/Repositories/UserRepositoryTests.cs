using FiapX.Domain.Entities;
using FiapX.Infrastructure.Persistence.Context;
using FiapX.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        var user = new User("test@example.com", "hash123", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("test@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
    {
        var user = new User("test@example.com", "hash123", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        var user = new User("existing@example.com", "hash123", "Existing User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var exists = await _repository.EmailExistsAsync("existing@example.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        var exists = await _repository.EmailExistsAsync("nonexistent@example.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldBeCaseInsensitive()
    {
        var user = new User("test@example.com", "hash123", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var exists = await _repository.EmailExistsAsync("TEST@EXAMPLE.COM");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
    {
        var user = new User("test@example.com", "hash123", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        var user = new User("new@example.com", "hash123", "New User");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var user1 = new User("user1@example.com", "hash1", "User One");
        var user2 = new User("user2@example.com", "hash2", "User Two");
        var user3 = new User("user3@example.com", "hash3", "User Three");

        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Email == "user1@example.com");
        result.Should().Contain(u => u.Email == "user2@example.com");
        result.Should().Contain(u => u.Email == "user3@example.com");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}