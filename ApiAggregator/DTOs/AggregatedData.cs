namespace ApiAggregator.DTOs
{
    public class AggregatedData
    {
        public WeatherResponse Weather { get; set; }
        public NewsResponse News { get; set; }
        public GithubResponse GithubResponse { get; set; }
        public CoinGeckoPriceResponse CoinGeckoPrice { get; set; }
    }
}

