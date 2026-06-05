namespace Server.Domain;

public interface IMarketDataService
{
    Task<decimal?> GetLivePrice(string ticker);
}