using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace API.Models
{
    /// <summary>
    /// Request model for credit card recommendations
    /// </summary>
    public class CreditCardRequest
    {
        /// <summary>
        /// Customer's full name
        /// </summary>
        /// <example>John Smith</example>
        [Required(ErrorMessage = "Name is required")]
        [MinLength(1)]
        public string Name { get; init; } = "";

        /// <summary>
        /// Credit score (0-700)
        /// </summary>
        /// <example>650</example>
        [Range(0, 700, ErrorMessage = "Score must be between 0 and 700")]
        public int Score { get; init; }

        /// <summary>
        /// Annual salary in GBP
        /// </summary>
        /// <example>35000</example>
        [Range(0, int.MaxValue, ErrorMessage = "Salary must be positive")]
        public int Salary { get; init; }
    }
}