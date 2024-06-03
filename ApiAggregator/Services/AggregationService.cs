using ApiAggregator.Configuration;
using ApiAggregator.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Services
{
    /// <summary>
    /// Service to aggregate data from multiple external APIs.
    /// </summary>
    public class AggregationService : IAggregationService
    {
        private readonly IWeatherService _weatherService;
        private readonly INewsService _newsService;
        private readonly IGitHubService _gitHubService;
        private readonly ICoinGeckoService _coinGeckoService;
        private readonly ApiSettings _apiSettings;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AggregationService> _logger;


        public AggregationService(
            IWeatherService weatherService,
            INewsService newsService,
            IGitHubService gitHubService,
            ICoinGeckoService coinGeckoService,
            IOptions<ApiSettings> apiSettings,
            IMemoryCache cache,
            ILogger<AggregationService> logger)
        {
            _weatherService = weatherService;
            _newsService = newsService;
            _gitHubService = gitHubService;
            _coinGeckoService = coinGeckoService;
            _apiSettings = apiSettings.Value;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets aggregated data from multiple APIs.
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
            _logger.LogInformation("Starting GetAggregatedDataAsync for location: {Location}, newsCountry: {NewsCountry}, gitHubUsername: {GitHubUsername}, cryptoIds: {CryptoIds}, cryptoCurrency: {CryptoCurrency}", location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);

            try
            {
                var weatherTask = _weatherService.GetWeatherAsync(location, _apiSettings.WeatherApiKey);
                var newsTask = _newsService.GetNewsAsync(newsCountry, "general", _apiSettings.NewsApiKey);
                var gitHubResponseTask = _gitHubService.GetUserAsync(gitHubUsername);
                var coinGeckoPriceTask = _coinGeckoService.GetPriceAsync(cryptoIds, cryptoCurrency);

                await Task.WhenAll(weatherTask, newsTask, gitHubResponseTask, coinGeckoPriceTask);

                var aggregatedData = new AggregatedData
                {
                    Weather = await weatherTask,
                    News = await newsTask,
                    GithubResponse = await gitHubResponseTask,
                    CoinGeckoPrice = await coinGeckoPriceTask,
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

                _cache.Set($"{location}_{newsCountry}_{gitHubUsername}_{cryptoIds}_{cryptoCurrency}", aggregatedData, cacheEntryOptions);

                _logger.LogInformation("Successfully retrieved aggregated data for location: {Location}, newsCountry: {NewsCountry}, gitHubUsername: {GitHubUsername}, cryptoIds: {CryptoIds}, cryptoCurrency: {CryptoCurrency}", location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);

                return aggregatedData;
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while fetching data for location: {Location}, newsCountry: {NewsCountry}, gitHubUsername: {GitHubUsername}, cryptoIds: {CryptoIds}, cryptoCurrency: {CryptoCurrency}", location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for location: {Location}, newsCountry: {NewsCountry}, gitHubUsername: {GitHubUsername}, cryptoIds: {CryptoIds}, cryptoCurrency: {CryptoCurrency}", location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);
                throw;
            }
        }
    }
}
