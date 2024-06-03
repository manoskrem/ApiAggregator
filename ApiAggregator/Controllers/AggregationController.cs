using ApiAggregator.DTOs;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AggregationController : ControllerBase
{
    private readonly IAggregationService _aggregationService;

    public AggregationController(IAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    /// <summary>
    /// Gets aggregated data from multiple APIs.
    /// </summary>
    /// <param name="location">The location for which to retrieve weather data. Example: "London".</param>
    /// <param name="newsCountry">The country code for which to retrieve news data. Example: "us" for the United States.</param>
    /// <param name="gitHubUsername">The GitHub username to retrieve user data. Example: "octocat".</param>
    /// <param name="cryptoIds">The cryptocurrency IDs to retrieve prices. Comma-separated list. Example: "bitcoin,ethereum".</param>
    /// <param name="cryptoCurrency">The currency for which to retrieve cryptocurrency prices. Example: "usd".</param>
    /// <returns>An aggregated data response containing weather, news, GitHub user, and cryptocurrency price information.</returns>
    /// <response code="200">Returns the aggregated data.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("aggregate")]
    [ProducesResponseType(typeof(AggregatedData), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAggregatedData([FromQuery] string location, [FromQuery] string newsCountry, [FromQuery] string gitHubUsername, [FromQuery] string cryptoIds, [FromQuery] string cryptoCurrency)
    {
        try
        {
            var aggregatedData = await _aggregationService.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);
            return Ok(aggregatedData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
