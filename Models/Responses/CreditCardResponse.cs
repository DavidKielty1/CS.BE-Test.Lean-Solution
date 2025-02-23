using API.Models.Common;

namespace API.Models.Responses
{
    /// <summary>
    /// Response containing credit card recommendations
    /// </summary>
    /// <example>
    /// {
    ///     "message": "Fetched from APIs",
    ///     "cards": [
    ///         {
    ///             "provider": "CSCards",
    ///             "name": "Super Saver Card",
    ///             "apr": 19.5,
    ///             "cardScore": 0.85
    ///         },
    ///         {
    ///             "provider": "ScoredCards",
    ///             "name": "Premium Rewards Card",
    ///             "apr": 21.9,
    ///             "cardScore": 0.75
    ///         }
    ///     ]
    /// }
    /// </example>
    public class CreditCardResponse
    {
        public string Message { get; init; } = "";
        public List<CreditCardRecommendation> Cards { get; init; } = new();
    }
}