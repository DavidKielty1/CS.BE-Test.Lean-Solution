using API.Models;
using API.Models.Common;
using API.Services;
using API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace API.Tests.Services;

public class CreditCardServiceTests
{
    private readonly Mock<IRedisService> _mockCache;
    private readonly Mock<ICardProviderService> _mockCardProvider;
    private readonly Mock<ILogger<CreditCardService>> _mockLogger;
    private readonly CreditCardService _service;

    public CreditCardServiceTests()
    {
        _mockCache = new Mock<IRedisService>();
        _mockCardProvider = new Mock<ICardProviderService>();
        _mockLogger = new Mock<ILogger<CreditCardService>>();
        _service = new CreditCardService(_mockCache.Object, _mockCardProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRecommendations_WhenCacheHit_ReturnsFromCache()
    {
        // Arrange
        var request = new CreditCardRequest { Name = "Test", Score = 700, Salary = 30000 };
        var cachedCards = new List<CreditCardRecommendation>
        {
            new() { Name = "Cached Card", Provider = "CSCards", Apr = 15.0m, CardScore = 8.0m }
        };

        _mockCache.Setup(x => x.GetRequestResults(request.Name, request.Score, request.Salary))
            .ReturnsAsync(cachedCards);

        // Act
        var (cards, fromCache) = await _service.GetRecommendations(request);

        // Assert
        Assert.True(fromCache);
        Assert.Equal(cachedCards, cards);
        _mockCardProvider.Verify(x => x.GetAllRecommendations(It.IsAny<CreditCardRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetRecommendations_WhenCacheMiss_CallsProvidersAndCaches()
    {
        // Arrange
        var request = new CreditCardRequest { Name = "Test", Score = 700, Salary = 30000 };
        var providerCards = new List<CreditCardRecommendation>
        {
            new() { Name = "New Card", Provider = "CSCards", Apr = 15.0m, CardScore = 8.0m }
        };

        _mockCache.Setup(x => x.GetRequestResults(request.Name, request.Score, request.Salary))
            .ReturnsAsync((List<CreditCardRecommendation>?)null);
        _mockCardProvider.Setup(x => x.GetAllRecommendations(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providerCards);

        // Act
        var (cards, fromCache) = await _service.GetRecommendations(request);

        // Assert
        Assert.False(fromCache);
        Assert.Equal(providerCards.Count, cards.Count);
        _mockCache.Verify(x => x.StoreRequestResults(request.Name, request.Score, request.Salary, It.IsAny<List<CreditCardRecommendation>>()), Times.Once);
    }

    [Fact]
    public async Task GetRecommendations_WhenCacheReadFails_FallsBackToProviders()
    {
        // Arrange
        var request = new CreditCardRequest { Name = "Test", Score = 700, Salary = 30000 };
        var providerCards = new List<CreditCardRecommendation>
        {
            new() { Name = "New Card", Provider = "CSCards", Apr = 15.0m, CardScore = 8.0m }
        };

        _mockCache.Setup(x => x.GetRequestResults(request.Name, request.Score, request.Salary))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection failed"));

        _mockCardProvider.Setup(x => x.GetAllRecommendations(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providerCards);

        // Act
        var (cards, fromCache) = await _service.GetRecommendations(request);

        // Assert
        Assert.False(fromCache);
        Assert.Equal(providerCards.Count, cards.Count);
        Assert.Equal(providerCards[0].Name, cards[0].Name);

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Redis cache unavailable")),
                It.IsAny<RedisConnectionException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRecommendations_WhenCacheWriteFails_ContinuesSuccessfully()
    {
        // Arrange
        var request = new CreditCardRequest { Name = "Test", Score = 700, Salary = 30000 };
        var providerCards = new List<CreditCardRecommendation>
        {
            new() { Name = "New Card", Provider = "CSCards", Apr = 15.0m, CardScore = 8.0m }
        };

        _mockCache.Setup(x => x.GetRequestResults(request.Name, request.Score, request.Salary))
            .ReturnsAsync((List<CreditCardRecommendation>?)null);

        _mockCache.Setup(x => x.StoreRequestResults(
                request.Name, request.Score, request.Salary, It.IsAny<List<CreditCardRecommendation>>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection failed"));

        _mockCardProvider.Setup(x => x.GetAllRecommendations(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providerCards);

        // Act
        var (cards, fromCache) = await _service.GetRecommendations(request);

        // Assert
        Assert.False(fromCache);
        Assert.Equal(providerCards.Count, cards.Count);

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to store results in cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRecommendations_WhenCacheThrowsUnexpectedError_FallsBackToProviders()
    {
        // Arrange
        var request = new CreditCardRequest { Name = "Test", Score = 700, Salary = 30000 };
        var providerCards = new List<CreditCardRecommendation>
        {
            new() { Name = "New Card", Provider = "CSCards", Apr = 15.0m, CardScore = 8.0m }
        };

        _mockCache.Setup(x => x.GetRequestResults(request.Name, request.Score, request.Salary))
            .ThrowsAsync(new InvalidOperationException("Unexpected cache error"));

        _mockCardProvider.Setup(x => x.GetAllRecommendations(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providerCards);

        // Act
        var (cards, fromCache) = await _service.GetRecommendations(request);

        // Assert
        Assert.False(fromCache);
        Assert.Equal(providerCards.Count, cards.Count);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error accessing cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}