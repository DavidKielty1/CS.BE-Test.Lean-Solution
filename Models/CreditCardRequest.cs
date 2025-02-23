using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreditCardRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; init; } = "";

        [Range(0, 700, ErrorMessage = "Score must be between 0 and 700")]
        public int Score { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Salary must be positive")]
        public int Salary { get; init; }
    }
}