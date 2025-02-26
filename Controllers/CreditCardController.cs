using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Models.Common;
using API.Services;
using API.Models.Responses;
using API.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using Prometheus;

namespace API.Controllers
{
    /// <summary>
    /// Entry point for credit card recommendation requests.
    /// Handles HTTP requests, caching, and orchestrates the recommendation process.
    /// </summary>
    [ApiController]
    [Route("creditcards")]
    [Produces("application/json")]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardService _service;
        private readonly ILogger<CreditCardController> _logger;

        // Define metrics
        private static readonly Counter RecommendationsRequested =
            Metrics.CreateCounter("creditcard_recommendations_requested", "Number of credit card recommendations requested");

        private static readonly Counter CacheHits =
            Metrics.CreateCounter("creditcard_cache_hits", "Number of cache hits for credit card recommendations");

        private static readonly Histogram ProcessingTime =
            Metrics.CreateHistogram("creditcard_processing_duration_seconds",
                "Time taken to process credit card recommendations");

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
        /// <response code="400">The request contained invalid parameters</response>
        /// <response code="503">Service unavailable</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreditCardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [SwaggerResponse(400, "The request contained invalid parameters")]
        [SwaggerResponse(503, "Service unavailable")]
        public async Task<IActionResult> ProcessCreditCard([FromBody] CreditCardRequest request)
        {
            using (ProcessingTime.NewTimer())
            {
                try
                {
                    RecommendationsRequested.Inc();

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(new ErrorResponse { Message = ModelState.Values.First().Errors.First().ErrorMessage });
                    }

                    var (cards, fromCache) = await _service.GetRecommendations(request);

                    if (fromCache)
                    {
                        CacheHits.Inc();
                    }

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
}