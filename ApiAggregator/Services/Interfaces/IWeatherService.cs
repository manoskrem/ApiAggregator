using ApiAggregator.DTOs;
using Refit;


namespace ApiAggregator.Services
{
    public interface IWeatherService
    {
        [Get("/data/2.5/weather")]
        Task<WeatherResponse> GetWeatherAsync([Query] string q, [Query] string appid);
    }

}
