using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models.CardProviders
{
    public class ScoredCardsResponse
    {
        public List<ScoredCard> Cards { get; set; } = new();
    }

    public class ScoredCard
    {
        [Required]
        [JsonPropertyName("card")]
        public string Card { get; set; } = "";

        [JsonPropertyName("apr")]
        public double Apr { get; set; }

        [Range(0.0, 1.0)]
        [JsonPropertyName("approvalRating")]
        public double ApprovalRating { get; set; }
    }
}