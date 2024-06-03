using System.Threading.Tasks;
using ApiAggregator.DTOs;
using Refit;

namespace ApiAggregator.Services
{
    public interface ICoinGeckoService
    {
        [Get("/simple/price")]
        Task<CoinGeckoPriceResponse> GetPriceAsync([Query] string ids, [Query] string vs_currencies);
    }
}
