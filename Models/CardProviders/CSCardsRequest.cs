using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models.CardProviders
{
    public class CSCardsRequest
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [Range(0, 700)]
        [JsonPropertyName("creditScore")]
        public int CreditScore { get; set; }
    }
}