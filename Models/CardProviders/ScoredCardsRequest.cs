using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models.CardProviders
{
    public class ScoredCardsRequest
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [Range(0, 700)]
        [JsonPropertyName("score")]
        public int Score { get; set; }

        [Range(0, int.MaxValue)]
        [JsonPropertyName("salary")]
        public int Salary { get; set; }
    }
}