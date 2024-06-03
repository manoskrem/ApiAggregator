using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using FluentAssertions;
using ApiAggregator.DTOs;
using ApiAggregator.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ApiAggregator.Configuration;

public class AggregationServiceTests
{
    private readonly IWeatherService _weatherService;
    private readonly INewsService _newsService;
    private readonly IGitHubService _gitHubService;
    private readonly ICoinGeckoService _coinGeckoService;
    private readonly IOptions<ApiSettings> _apiSettings;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AggregationService> _logger;
    private readonly AggregationService _innerService;
    private readonly AggregationServiceDecorator _service;

    public AggregationServiceTests()
    {
        _weatherService = Substitute.For<IWeatherService>();
        _newsService = Substitute.For<INewsService>();
        _gitHubService = Substitute.For<IGitHubService>();
        _coinGeckoService = Substitute.For<ICoinGeckoService>();
        _apiSettings = Options.Create(new ApiSettings
        {
            WeatherApiKey = "test-weather-api-key",
            NewsApiKey = "test-news-api-key"
        });
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _logger = Substitute.For<ILogger<AggregationService>>();

        _innerService = new AggregationService(_weatherService, _newsService, _gitHubService, _coinGeckoService, _apiSettings, _memoryCache,_logger);
        _service = new AggregationServiceDecorator(_innerService, _memoryCache);
    }

    [Fact]
    public async Task GetAggregatedDataAsync_ShouldReturnAggregatedData_WhenAPICallsAreSuccessful()
    {
        // Arrange
        var location = "test-location";
        var newsCountry = "test-country";
        var gitHubUsername = "octocat";
        var cryptoIds = "bitcoin,ethereum";
        var cryptoCurrency = "usd";

        var weatherResponse = new WeatherResponse
        {
            Coord = new Coord { Lon = -0.13f, Lat = 51.51f },
            Weather = new[]
            {
                new Weather { Id = 800, Main = "Clear", Description = "clear sky", Icon = "01d" }
            },
            Base = "stations",
            Main = new Main
            {
                Temp = 289.92f,
                Feels_like = 287.15f,
                Temp_min = 288.71f,
                Temp_max = 290.93f,
                Pressure = 1012,
                Humidity = 81
            },
            Visibility = 10000,
            Wind = new Wind { Speed = 4.1f, Deg = 80 },
            Clouds = new Clouds { All = 0 },
            Dt = 1560350645,
            Sys = new Sys
            {
                Type = 1,
                Id = 1414,
                Country = "GB",
                Sunrise = 1560300463,
                Sunset = 1560362253
            },
            Timezone = 3600,
            Id = 2643743,
            Name = "London",
            Cod = 200
        };

        var newsResponse = new NewsResponse
        {
            Status = "ok",
            TotalResults = 1,
            Articles = new List<Article>
            {
                new Article
                {
                    Source = new Source { Id = "bbc-news", Name = "BBC News" },
                    Author = "BBC News",
                    Title = "Test Article",
                    Description = "This is a test article",
                    Url = "http://www.bbc.co.uk/news/test-article",
                    UrlToImage = "http://www.bbc.co.uk/news/test-article/image.jpg",
                    PublishedAt = "2023-05-01T12:00:00Z",
                    Content = "This is the content of the test article"
                }
            }
        };

        var gitHubResponse = new GithubResponse
        {
            Login = "octocat",
            Name = "The Octocat",
            Company = "GitHub",
            Blog = "https://github.blog",
            PublicRepos = 8,
            Followers = 3939,
            Following = 9
        };

        var coinGeckoPriceResponse = new CoinGeckoPriceResponse
        {
            ["bitcoin"] = new Dictionary<string, decimal> { ["usd"] = 45000.0m },
            ["ethereum"] = new Dictionary<string, decimal> { ["usd"] = 3000.0m }
        };

        _weatherService.GetWeatherAsync(location, _apiSettings.Value.WeatherApiKey).Returns(Task.FromResult(weatherResponse));
        _newsService.GetNewsAsync(newsCountry, "general", _apiSettings.Value.NewsApiKey).Returns(Task.FromResult(newsResponse));
        _gitHubService.GetUserAsync(gitHubUsername).Returns(Task.FromResult(gitHubResponse));
        _coinGeckoService.GetPriceAsync(cryptoIds, cryptoCurrency).Returns(Task.FromResult(coinGeckoPriceResponse));

        // Act
        var result = await _service.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);

        // Assert
        result.Should().NotBeNull();
        result.Weather.Should().BeEquivalentTo(weatherResponse);
        result.News.Should().BeEquivalentTo(newsResponse);
        result.GithubResponse.Should().BeEquivalentTo(gitHubResponse);
        result.CoinGeckoPrice.Should().BeEquivalentTo(coinGeckoPriceResponse);
    }

    [Fact]
    public async Task GetAggregatedDataAsync_ShouldCacheResult_AfterFirstCall()
    {
        // Arrange
        var location = "test-location";
        var newsCountry = "test-country";
        var gitHubUsername = "octocat";
        var cryptoIds = "bitcoin,ethereum";
        var cryptoCurrency = "usd";

        var weatherResponse = new WeatherResponse
        {
            Coord = new Coord { Lon = -0.13f, Lat = 51.51f },
            Weather = new[]
            {
                new Weather { Id = 800, Main = "Clear", Description = "clear sky", Icon = "01d" }
            },
            Base = "stations",
            Main = new Main
            {
                Temp = 289.92f,
                Feels_like = 287.15f,
                Temp_min = 288.71f,
                Temp_max = 290.93f,
                Pressure = 1012,
                Humidity = 81
            },
            Visibility = 10000,
            Wind = new Wind { Speed = 4.1f, Deg = 80 },
            Clouds = new Clouds { All = 0 },
            Dt = 1560350645,
            Sys = new Sys
            {
                Type = 1,
                Id = 1414,
                Country = "GB",
                Sunrise = 1560300463,
                Sunset = 1560362253
            },
            Timezone = 3600,
            Id = 2643743,
            Name = "London",
            Cod = 200
        };

        var newsResponse = new NewsResponse
        {
            Status = "ok",
            TotalResults = 1,
            Articles = new List<Article>
            {
                new Article
                {
                    Source = new Source { Id = "bbc-news", Name = "BBC News" },
                    Author = "BBC News",
                    Title = "Test Article",
                    Description = "This is a test article",
                    Url = "http://www.bbc.co.uk/news/test-article",
                    UrlToImage = "http://www.bbc.co.uk/news/test-article/image.jpg",
                    PublishedAt = "2023-05-01T12:00:00Z",
                    Content = "This is the content of the test article"
                }
            }
        };

        var gitHubResponse = new GithubResponse
        {
            Login = "octocat",
            Name = "The Octocat",
            Company = "GitHub",
            Blog = "https://github.blog",
            PublicRepos = 8,
            Followers = 3939,
            Following = 9
        };

        var coinGeckoPriceResponse = new CoinGeckoPriceResponse
        {
            ["bitcoin"] = new Dictionary<string, decimal> { ["usd"] = 45000.0m },
            ["ethereum"] = new Dictionary<string, decimal> { ["usd"] = 3000.0m }
        };

        _weatherService.GetWeatherAsync(location, _apiSettings.Value.WeatherApiKey).Returns(Task.FromResult(weatherResponse));
        _newsService.GetNewsAsync(newsCountry, "general", _apiSettings.Value.NewsApiKey).Returns(Task.FromResult(newsResponse));
        _gitHubService.GetUserAsync(gitHubUsername).Returns(Task.FromResult(gitHubResponse));
        _coinGeckoService.GetPriceAsync(cryptoIds, cryptoCurrency).Returns(Task.FromResult(coinGeckoPriceResponse));

        // Act
        var result1 = await _service.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);
        var result2 = await _service.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().BeEquivalentTo(result2);

        _weatherService.Received(1).GetWeatherAsync(location, _apiSettings.Value.WeatherApiKey);
        _newsService.Received(1).GetNewsAsync(newsCountry, "general", _apiSettings.Value.NewsApiKey);
        _gitHubService.Received(1).GetUserAsync(gitHubUsername);
        _coinGeckoService.Received(1).GetPriceAsync(cryptoIds, cryptoCurrency);
    }
}
