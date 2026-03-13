using FiapX.Infrastructure.Cache;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace FiapX.Infrastructure.Tests.Cache;

public class RedisCacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();

        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _cacheService = new RedisCacheService(_redisMock.Object);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnValue()
    {
        var key = "test-key";
        var testData = new TestData { Name = "Test", Value = 123 };
        var serialized = JsonSerializer.Serialize(testData);

        _databaseMock
            .Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)serialized);

        var result = await _cacheService.GetAsync<TestData>(key);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ShouldReturnNull()
    {
        var key = "non-existent-key";

        _databaseMock
            .Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _cacheService.GetAsync<string>(key);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldCallRedisStringSet()
    {
        var key = "test-key";
        var value = new TestData { Name = "Test" };
        var expiration = TimeSpan.FromMinutes(5);

        _databaseMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _cacheService.SetAsync(key, value, expiration);

        _databaseMock.Verify(
            x => x.StringSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallRedisKeyDelete()
    {
        var key = "test-key";

        _databaseMock
            .Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _cacheService.RemoveAsync(key);

        _databaseMock.Verify(
            x => x.KeyDeleteAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
    {
        var key = "existing-key";

        _databaseMock
            .Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _cacheService.ExistsAsync(key);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentKey_ShouldReturnFalse()
    {
        var key = "non-existent-key";

        _databaseMock
            .Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var result = await _cacheService.ExistsAsync(key);

        result.Should().BeFalse();
    }

    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}