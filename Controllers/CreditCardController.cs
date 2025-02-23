using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Models.Common;
using API.Services;
using API.Models.Responses;
using API.Services.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Entry point for credit card recommendation requests.
    /// Handles HTTP requests, caching, and orchestrates the recommendation process.
    /// </summary>
    [ApiController]
    [Route("api/credit-cards")]
    [Produces("application/json")]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardService _service;
        private readonly ILogger<CreditCardController> _logger;

        public CreditCardController(ICreditCardService service, ILogger<CreditCardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Process a credit card recommendation request
        /// </summary>
        /// <param name="request">Customer details for credit card assessment</param>
        /// <returns>List of recommended credit cards with eligibility scores</returns>
        /// <response code="200">Returns the credit card recommendations</response>
        /// <response code="400">If the request is invalid or no cards found</response>
        /// <response code="503">If the service is temporarily unavailable</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(CreditCardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ProcessCreditCard([FromBody] CreditCardRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse { Message = ModelState.Values.First().Errors.First().ErrorMessage });
                }

                var (cards, fromCache) = await _service.GetRecommendations(request);

                if (!cards.Any())
                {
                    return BadRequest(new ErrorResponse { Message = "No credit card recommendations found" });
                }

                return Ok(new CreditCardResponse
                {
                    Message = fromCache ? "Retrieved from cache" : "Fetched from APIs",
                    Cards = cards
                });
            }
            catch (TimeoutException)
            {
                return StatusCode(503, new ErrorResponse { Message = "Service unavailable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credit card request");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
    }
}