using API.Models.Common;

namespace API.Models.Responses
{
    public class CreditCardResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<CreditCardRecommendation>? Cards { get; set; }
    }
}