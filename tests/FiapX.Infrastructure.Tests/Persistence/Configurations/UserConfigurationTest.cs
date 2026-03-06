using FiapX.Domain.Entities;
using FiapX.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FiapX.Infrastructure.Tests.Persistence.Configurations;

public class UserConfigurationTests : IDisposable
{
    private readonly AppDbContext _context;

    public UserConfigurationTests()
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
    public void UserConfiguration_ShouldMapToTable_Users()
    {
        var entityType = _context.Model.FindEntityType(typeof(User));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Users");
    }

    [Fact]
    public void UserConfiguration_ShouldHavePrimaryKey_Id()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;
        var pk = entityType.FindPrimaryKey();

        pk.Should().NotBeNull();
        pk!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void UserConfiguration_Id_ShouldBeValueGeneratedNever()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;
        var idProperty = entityType.FindProperty("Id")!;

        idProperty.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public void UserConfiguration_Email_ShouldBeRequired_WithMaxLength256()
    {
        var property = _context.Model.FindEntityType(typeof(User))!.FindProperty("Email")!;

        property.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void UserConfiguration_PasswordHash_ShouldBeRequired_WithMaxLength255()
    {
        var property = _context.Model.FindEntityType(typeof(User))!.FindProperty("PasswordHash")!;

        property.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void UserConfiguration_Name_ShouldBeRequired_WithMaxLength200()
    {
        var property = _context.Model.FindEntityType(typeof(User))!.FindProperty("Name")!;

        property.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public void UserConfiguration_IsActive_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(User))!.FindProperty("IsActive")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UserConfiguration_CreatedAt_ShouldBeRequired()
    {
        var property = _context.Model.FindEntityType(typeof(User))!.FindProperty("CreatedAt")!;

        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UserConfiguration_UpdatedAt_ShouldBeNullable()
    {
        var property = _context.Model.FindEntityType(typeof(User))!.FindProperty("UpdatedAt")!;

        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void UserConfiguration_Email_ShouldHaveUniqueIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Email"));

        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void UserConfiguration_IsActive_ShouldHaveIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "IsActive"));

        index.Should().NotBeNull();
    }

    [Fact]
    public void UserConfiguration_CreatedAt_ShouldHaveIndex()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "CreatedAt"));

        index.Should().NotBeNull();
    }

    [Fact]
    public void UserConfiguration_ShouldHaveOneToManyRelationship_WithVideos()
    {
        var entityType = _context.Model.FindEntityType(typeof(User))!;
        var navigation = entityType.GetNavigations()
            .FirstOrDefault(n => n.Name == "Videos");

        navigation.Should().NotBeNull();
    }

    [Fact]
    public async Task UserConfiguration_ShouldPersistAndRetrieveUser()
    {
        var user = new User("persist@example.com", "hash123", "Persist User");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        var retrieved = await _context.Users.FindAsync(user.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("persist@example.com");
        retrieved.Name.Should().Be("Persist User");
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UserConfiguration_Email_ShouldEnforceUniqueness()
    {
        var user1 = new User("unique@example.com", "hash1", "User One");
        var user2 = new User("unique@example.com", "hash2", "User Two");

        await _context.Users.AddAsync(user1);
        await _context.SaveChangesAsync();

        await _context.Users.AddAsync(user2);
        var act = async () => await _context.SaveChangesAsync();

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UserConfiguration_ShouldPersistUpdatedAt_WhenModified()
    {
        var user = new User("update@example.com", "hash", "Update User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var retrieved = await _context.Users.FindAsync(user.Id);
        retrieved!.UpdateName("Updated Name");
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updated = await _context.Users.FindAsync(user.Id);

        updated!.UpdatedAt.Should().NotBeNull();
    }
}
