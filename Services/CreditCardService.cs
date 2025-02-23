using API.Models;
using API.Models.Common;
using API.Services.Interfaces;
using StackExchange.Redis;

namespace API.Services
{
    /// <summary>
    /// Core business logic for credit card recommendations.
    /// Manages caching strategy and coordinates between different card providers.
    /// </summary>
    public class CreditCardService : ICreditCardService
    {
        private readonly IRedisService _cache;
        private readonly ICardProviderService _cardProvider;
        private readonly ILogger<CreditCardService> _logger;

        public CreditCardService(
            IRedisService cache,
            ICardProviderService cardProvider,
            ILogger<CreditCardService> logger)
        {
            _cache = cache;
            _cardProvider = cardProvider;
            _logger = logger;
        }

        public async Task<(List<CreditCardRecommendation> cards, bool fromCache)> GetRecommendations(CreditCardRequest request)
        {
            try
            {
                // Step 1: Try to get cached results first
                var cached = await _cache.GetRequestResults(request.Name, request.Score, request.Salary);
                if (cached?.Any() == true)
                {
                    return (cached, true);
                }
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis cache unavailable, falling back to direct API calls");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error accessing cache, falling back to direct API calls");
            }

            // Step 2: Get fresh recommendations from providers
            var cards = await _cardProvider.GetAllRecommendations(request, default);
            if (cards.Any())
            {
                // Step 3: Calculate scores
                var scored = CardScoreCalculator.CalculateNormalizedScores(cards, _logger);

                try
                {
                    // Try to cache the results, but don't fail if cache is unavailable
                    await _cache.StoreRequestResults(request.Name, request.Score, request.Salary, scored);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store results in cache");
                }

                return (scored, false);
            }

            return (new List<CreditCardRecommendation>(), false);
        }
    }
}