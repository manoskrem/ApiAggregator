using ApiAggregator.DTOs;
using ApiAggregator.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NSubstitute;

public class AggregationControllerTests
{
    private readonly IAggregationService _aggregationService;
    private readonly AggregationController _controller;

    public AggregationControllerTests()
    {
        _aggregationService = Substitute.For<IAggregationService>();
        _controller = new AggregationController(_aggregationService);
    }

    [Fact]
    public async Task GetAggregatedData_ShouldReturnOkResult_WithAggregatedData()
    {
        // Arrange
        var location = "test-location";
        var newsCountry = "test-country";
        var gitHubUsername = "octocat";
        var cryptoIds = "bitcoin,ethereum";
        var cryptoCurrency = "usd";

        var aggregatedData = new AggregatedData
        {
            Weather = new WeatherResponse
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
            },
            News = new NewsResponse
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
            },
            GithubResponse = new GithubResponse
            {
                Login = "octocat",
                Name = "The Octocat",
                Company = "GitHub",
                Blog = "https://github.blog",
                PublicRepos = 8,
                Followers = 3939,
                Following = 9
            },
            CoinGeckoPrice = new CoinGeckoPriceResponse
            {
                ["bitcoin"] = new Dictionary<string, decimal> { ["usd"] = 45000.0m },
                ["ethereum"] = new Dictionary<string, decimal> { ["usd"] = 3000.0m }
            }
        };

        _aggregationService.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency).Returns(Task.FromResult(aggregatedData));

        // Act
        var result = await _controller.GetAggregatedData(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(aggregatedData);
    }

    [Fact]
    public async Task GetAggregatedData_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var location = "test-location";
        var newsCountry = "test-country";
        var gitHubUsername = "octocat";
        var cryptoIds = "bitcoin,ethereum";
        var cryptoCurrency = "usd";

        _aggregationService.GetAggregatedDataAsync(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency).Returns(Task.FromException<AggregatedData>(new Exception("Test exception")));

        // Act
        var result = await _controller.GetAggregatedData(location, newsCountry, gitHubUsername, cryptoIds, cryptoCurrency) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(500);

        var errorResponse = JObject.FromObject(result.Value);
        errorResponse["error"]?.ToString().Should().Be("Test exception");
    }
    }
