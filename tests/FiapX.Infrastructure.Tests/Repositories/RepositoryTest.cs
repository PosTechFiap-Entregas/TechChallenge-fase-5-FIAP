using FiapX.Domain.Entities;
using FiapX.Infrastructure.Persistence.Context;
using FiapX.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Repository<User> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new Repository<User>(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private User MakeUser(string email = "test@example.com", string name = "Test User")
        => new User(email, "hash123", name);

    [Fact]
    public void Repository_Constructor_ShouldInitializeWithContext()
    {
        var repo = new Repository<User>(_context);
        repo.Should().NotBeNull();
    }

    [Fact]
    public async Task Repository_AddAsync_ShouldAddEntityToContext()
    {
        var user = MakeUser("add@example.com");

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var found = await _context.Users.FindAsync(user.Id);
        found.Should().NotBeNull();
        found!.Email.Should().Be("add@example.com");
    }

    [Fact]
    public async Task Repository_AddAsync_ShouldTrackEntityAsAdded_BeforeSave()
    {
        var user = MakeUser("track-add@example.com");

        await _repository.AddAsync(user);

        var entry = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == user.Id);

        entry.Should().NotBeNull();
        entry!.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task Repository_AddAsync_WithCancellationToken_ShouldRespectToken()
    {
        var user = MakeUser("cancel-add@example.com");
        using var cts = new CancellationTokenSource();

        var act = async () => await _repository.AddAsync(user, cts.Token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var user = MakeUser("getbyid@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("getbyid@example.com");
    }

    [Fact]
    public async Task Repository_GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_WithEmptyGuid_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_WithCancellationToken_ShouldRespectToken()
    {
        var user = MakeUser("cancel-get@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        using var cts = new CancellationTokenSource();

        var result = await _repository.GetByIdAsync(user.Id, cts.Token);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Repository_GetAllAsync_ShouldReturnAllEntities()
    {
        var user1 = MakeUser("all1@example.com", "User One");
        var user2 = MakeUser("all2@example.com", "User Two");
        var user3 = MakeUser("all3@example.com", "User Three");

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.AddAsync(user3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Repository_GetAllAsync_ShouldReturnEmptyCollection_WhenNoEntities()
    {
        var result = await _repository.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetAllAsync_WithCancellationToken_ShouldRespectToken()
    {
        using var cts = new CancellationTokenSource();

        var act = async () => await _repository.GetAllAsync(cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Repository_UpdateAsync_ShouldMarkEntityAsModified()
    {
        var user = MakeUser("update@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        user.UpdateName("Updated Name");
        await _repository.UpdateAsync(user);

        var entry = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == user.Id);

        entry.Should().NotBeNull();
        entry!.State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task Repository_UpdateAsync_ShouldPersistChanges_AfterSave()
    {
        var user = MakeUser("persist-update@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _repository.GetByIdAsync(user.Id);
        retrieved!.UpdateName("Persisted Update");
        await _repository.UpdateAsync(retrieved);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updated = await _repository.GetByIdAsync(user.Id);

        updated!.Name.Should().Be("Persisted Update");
    }

    [Fact]
    public async Task Repository_UpdateAsync_ShouldReturnCompletedTask()
    {
        var user = MakeUser("task-update@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var task = _repository.UpdateAsync(user);

        task.Should().NotBeNull();
        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_ShouldRemoveEntity_AfterSave()
    {
        var user = MakeUser("delete@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var toDelete = await _repository.GetByIdAsync(user.Id);
        await _repository.DeleteAsync(toDelete!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAsync_ShouldMarkEntityAsDeleted_BeforeSave()
    {
        var user = MakeUser("track-delete@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(user);

        var entry = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == user.Id);

        entry.Should().NotBeNull();
        entry!.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task Repository_DeleteAsync_ShouldReturnCompletedTask()
    {
        var user = MakeUser("task-delete@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var task = _repository.DeleteAsync(user);

        task.Should().NotBeNull();
        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        var user = MakeUser("exists@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var exists = await _repository.ExistsAsync(user.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsAsync_ShouldReturnFalse_WhenEntityNotExists()
    {
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ExistsAsync_ShouldReturnFalse_AfterDelete()
    {
        // Arrange
        var user = MakeUser("exists-delete@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var toDelete = await _repository.GetByIdAsync(user.Id);
        await _repository.DeleteAsync(toDelete!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var exists = await _repository.ExistsAsync(user.Id);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ExistsAsync_WithCancellationToken_ShouldRespectToken()
    {
        // Arrange
        var user = MakeUser("cancel-exists@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        using var cts = new CancellationTokenSource();

        // Act
        var exists = await _repository.ExistsAsync(user.Id, cts.Token);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsAsync_WithEmptyGuid_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.Empty);

        // Assert
        exists.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Cenários combinados
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Repository_AddThenGetAll_ShouldReflectAddedEntities()
    {
        // Arrange
        var users = Enumerable.Range(1, 5)
            .Select(i => MakeUser($"bulk{i}@example.com", $"Bulk User {i}"))
            .ToList();

        foreach (var u in users)
            await _repository.AddAsync(u);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var all = await _repository.GetAllAsync();

        // Assert
        all.Should().HaveCount(5);
    }

    [Fact]
    public async Task Repository_UpdateThenGetById_ShouldReturnUpdatedEntity()
    {
        // Arrange
        var user = MakeUser("round-trip@example.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var fetched = await _repository.GetByIdAsync(user.Id);
        fetched!.UpdateName("Round Trip Updated");
        await _repository.UpdateAsync(fetched);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result!.Name.Should().Be("Round Trip Updated");
    }
}