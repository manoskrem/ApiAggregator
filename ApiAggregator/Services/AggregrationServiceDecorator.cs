using ApiAggregator.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ApiAggregator.Services
{
    /// <summary>
    /// Decorator class for <see cref="IAggregationService"/> that adds caching functionality.
    /// </summary>
    public class AggregationServiceDecorator : IAggregationService
    {
        private readonly IAggregationService _innerService;
        private readonly IMemoryCache _cache;

        public AggregationServiceDecorator(IAggregationService innerService, IMemoryCache cache)
        {
            _innerService = innerService;
            _cache = cache;
        }

        /// <summary>
        /// Gets aggregated data from multiple APIs with caching.
        /// </summary>
        /// <param name="location">The location for which to retrieve weather data. Example: "London".</param>
        /// <param name="newsCountry">The country code for which to retrieve news data. Example: "us".</param>
        /// <param name="gitHubUsername">The GitHub username to retrieve user data. Example: "octocat".</param>
        /// <param name="cryptoIds">The cryptocurrency IDs to retrieve prices. Comma-separated list. Example: "bitcoin,ethereum".</param>
        /// <param name="cryptoCurrency">The currency for which to retrieve cryptocurrency prices. Example: "usd".</param>
        /// <returns>An aggregated data response containing weather, news, GitHub user, and cryptocurrency price information.</returns>
        /// <exception cref="Refit.ApiException">Thrown when an API error occurs.</exception>
        /// <exception cref="Exception">Thrown when a general error occurs.</exception>
        public async Task<AggregatedData> GetAggregatedDataAsync(string location, string newsCountry, string gitHubUsername, string cryptoIds, string cryptoCurrency)
        {
            var cacheKey = $"{location}_{newsCountry}_{gitHubUsername}_{cryptoIds}_{cryptoCurrency}";

            if (_cache.TryGetValue(cacheKey, out AggregatedData cachedData))
            {
                return cachedData;
            }

            var aggregatedData = await _innerService.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(cacheKey, aggregatedData, cacheEntryOptions);

            return aggregatedData;
        }
    }
}
