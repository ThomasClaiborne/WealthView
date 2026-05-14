using Server.Data;
using Server.Models;

namespace Server.Domain;

public class SecurityService : ISecurityService
{
    private readonly ISecurityRepository _securityRepo;
    private readonly IMarketDataService  _marketData;

    public SecurityService(ISecurityRepository securityRepo, IMarketDataService marketData)
    {
        _securityRepo = securityRepo;
        _marketData   = marketData;
    }

    public async Task<Result<List<Security>>> GetAll()
    {
        var securities = await _securityRepo.GetAll();

        foreach (var security in securities)
        {
            bool needsRefresh = security.LastPrice == null
                             || security.PriceFetchedAt?.Date != DateTime.UtcNow.Date;

            if (needsRefresh)
            {
                var freshPrice = await _marketData.GetLivePrice(security.Ticker);
                if (freshPrice.HasValue)
                {
                    await _securityRepo.UpdatePrice(security.Ticker, freshPrice.Value);

                    // Update in-memory object so the response reflects the fresh price
                    // (UpdatePrice writes directly to DB — doesn't update this instance)
                    security.LastPrice      = freshPrice.Value;
                    security.PriceFetchedAt = DateTime.UtcNow;
                }
                await Task.Delay(1200);
            }
        }

        var result = new Result<List<Security>>();
        result.Payload = securities;
        return result;
    }

    public async Task<Result<Security>> GetByTicker(string ticker)
    {
        var security = await _securityRepo.GetByTicker(ticker);
        var result   = new Result<Security>();

        if (security == null)
        {
            result.AddMessage($"Ticker '{ticker}' not found.", ResultType.NotFound);
            return result;
        }

        result.Payload = security;
        return result;
    }
}