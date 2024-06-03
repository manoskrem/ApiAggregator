using ApiAggregator.DTOs;

namespace ApiAggregator.Services
{

    public interface IAggregationService
    {
        Task<AggregatedData> GetAggregatedDataAsync(string location, string newsCountry, string gitHubUsername, string cryptoIds, string cryptoCurrency);
    }

}
