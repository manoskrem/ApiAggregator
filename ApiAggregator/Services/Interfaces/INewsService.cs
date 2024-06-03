using ApiAggregator.DTOs;
using Refit;



namespace ApiAggregator.Services
{
    public interface INewsService
    {
        [Get("/v2/top-headlines")]
        Task<NewsResponse> GetNewsAsync([Query] string country, [Query] string category, [Query] string apiKey);
    }

}
