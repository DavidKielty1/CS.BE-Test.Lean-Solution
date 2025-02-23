using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models.CardProviders
{
    public class CSCardsResponse
    {
        [JsonPropertyName("cards")]
        public List<CSCard> Cards { get; set; } = new();
    }

    public class CSCard
    {
        [Required]
        [JsonPropertyName("cardName")]
        public string CardName { get; set; } = "";

        [JsonPropertyName("apr")]
        public double Apr { get; set; }

        [Range(0.0, 10.0)]
        [JsonPropertyName("eligibility")]
        public double Eligibility { get; set; }
    }
}