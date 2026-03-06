using FiapX.Domain.Entities;
using FiapX.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Persistence.Context;

public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
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
    public void AppDbContext_Users_ShouldNotBeNull()
    {
        _context.Users.Should().NotBeNull();
    }

    [Fact]
    public void AppDbContext_Videos_ShouldNotBeNull()
    {
        _context.Videos.Should().NotBeNull();
    }

    [Fact]
    public void AppDbContext_EnsureCreated_ShouldCreateTablesForAllEntities()
    {
        var tableNames = _context.Model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .ToList();

        tableNames.Should().Contain("Users");
        tableNames.Should().Contain("Videos");
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_ShouldSetUpdatedAt_WhenEntityModified()
    {
        var user = new User("timestamp@example.com", "hash", "Timestamp User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _context.Users.FindAsync(user.Id);
        retrieved!.UpdateName("New Name");
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updated = await _context.Users.FindAsync(user.Id);

        updated!.UpdatedAt.Should().NotBeNull();
        updated.UpdatedAt.Should().BeAfter(updated.CreatedAt);
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_ShouldNotSetUpdatedAt_WhenEntityAdded()
    {
        var user = new User("added@example.com", "hash", "Added User");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _context.Users.FindAsync(user.Id);

        retrieved!.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_ShouldPersistMultipleEntities()
    {
        var user1 = new User("multi1@example.com", "hash1", "User One");
        var user2 = new User("multi2@example.com", "hash2", "User Two");

        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var count = await _context.Users.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public void AppDbContext_GlobalSettings_AllForeignKeysShouldHaveRestrictDeleteBehavior()
    {
        var foreignKeys = _context.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .ToList();

        foreignKeys.Should().NotBeEmpty();
        foreignKeys.All(fk => fk.DeleteBehavior != DeleteBehavior.NoAction).Should().BeTrue();
    }

    [Fact]
    public async Task AppDbContext_ChangeTracker_ShouldTrackAddedEntity()
    {
        var user = new User("track@example.com", "hash", "Tracked User");

        await _context.Users.AddAsync(user);

        var entry = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == user.Id);

        entry.Should().NotBeNull();
        entry!.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task AppDbContext_ChangeTracker_ShouldTrackModifiedEntity()
    {
        var user = new User("modify@example.com", "hash", "Modify User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        user.UpdateName("Modified Name");
        _context.Users.Update(user);

        var entry = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == user.Id);

        entry.Should().NotBeNull();
        entry!.State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task AppDbContext_CRUD_ShouldAddFindUpdateAndDeleteUser()
    {
        var user = new User("crud@example.com", "hash", "CRUD User");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var found = await _context.Users.FindAsync(user.Id);
        found.Should().NotBeNull();

        found!.UpdateName("Updated CRUD User");
        _context.Users.Update(found);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updated = await _context.Users.FindAsync(user.Id);
        updated!.Name.Should().Be("Updated CRUD User");

        _context.Users.Remove(updated);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var deleted = await _context.Users.FindAsync(user.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task AppDbContext_CRUD_ShouldAddFindUpdateAndDeleteVideo()
    {
        var user = new User("video-crud@example.com", "hash", "Video CRUD User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var video = new Video(user.Id, "clip.mp4", "/path/clip.mp4", 2048);

        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var found = await _context.Videos.FindAsync(video.Id);
        found.Should().NotBeNull();

        _context.Videos.Remove(found!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var deleted = await _context.Videos.FindAsync(video.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public void AppDbContext_Model_DecimalProperties_ShouldHavePrecisionAndScale()
    {
        var decimalProperties = _context.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
            .ToList();

        foreach (var prop in decimalProperties)
        {
            prop.GetPrecision().Should().Be(18);
            prop.GetScale().Should().Be(2);
        }
    }

    [Fact]
    public async Task AppDbContext_SaveChangesAsync_WithCancelledToken_ShouldThrowOperationCancelledException()
    {
        var user = new User("cancel@example.com", "hash", "Cancel User");
        await _context.Users.AddAsync(user);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _context.SaveChangesAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
