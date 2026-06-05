using System.Text.Json;
using Server.Domain;

namespace Server.Domain;

public class MarketDataService : IMarketDataService
{
    private readonly HttpClient    _httpClient;
    private readonly string        _apiKey;

    public MarketDataService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey     = config["AlphaVantage:ApiKey"]!;
    }

    public async Task<decimal?> GetLivePrice(string ticker)
    {
        try
        {
            var url = $"https://www.alphavantage.co/query" +
                      $"?function=GLOBAL_QUOTE&symbol={ticker}&apikey={_apiKey}";

            var json = await _httpClient.GetStringAsync(url);
            var doc  = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Rate limited or malformed response — no "Global Quote" key
            if (!root.TryGetProperty("Global Quote", out var quote))
                return null;

            if (!quote.TryGetProperty("05. price", out var priceEl))
                return null;

            var priceStr = priceEl.GetString();
            if (!decimal.TryParse(priceStr, out var price) || price == 0)
                return null;

            return price;
        }
        catch
        {
            return null;
        }
    }
}