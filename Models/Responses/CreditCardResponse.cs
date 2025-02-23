using API.Models.Common;
using System.Text.Json.Serialization;

namespace API.Models.Responses
{
    /// <summary>
    /// Response containing credit card recommendations
    /// </summary>
    /// <example>
    /// {
    ///     [Dev: "message": "Fetched from APIs",  // or "Retrieved from cache"]
    ///     "cards": [
    ///         {
    ///             "provider": "ScoredCards",    // Card provider name
    ///             "name": "ScoredCard Builder",
    ///             "apr": 19.4,                  // Annual Percentage Rate
    ///             "cardScore": 0.212           // Normalized eligibility score (0-1)
    ///         },
    ///         {
    ///             "provider": "CSCards",
    ///             "name": "SuperSaver Card",
    ///             "apr": 21.4,
    ///             "cardScore": 0.137
    ///         },
    ///         {
    ///             "provider": "CSCards",
    ///             "name": "SuperSpender Card",
    ///             "apr": 19.2,
    ///             "cardScore": 0.135
    ///         }
    ///     ]
    /// }
    /// </example>
    public class CreditCardResponse
    {
        /// <summary>
        /// Indicates whether the response was retrieved from cache or fetched from APIs
        /// </summary>
        /// <example>Fetched from APIs</example>
        [JsonPropertyName("message")]
        public string Message { get; init; } = "";

        /// <summary>
        /// List of recommended credit cards with their scores
        /// </summary>
        [JsonPropertyName("cards")]
        public List<CreditCardRecommendation> Cards { get; init; } = new();
    }
}