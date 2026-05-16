namespace Server.Domain;

using Server.Data;
using Server.DTOs.Responses;
using Server.Models;

public class HoldingService : IHoldingService
{
    private readonly IHoldingRepository   _holdingRepo;
    private readonly IPortfolioRepository _portfolioRepo;

    public HoldingService(IHoldingRepository holdingRepo, IPortfolioRepository portfolioRepo)
    {
        _holdingRepo   = holdingRepo;
        _portfolioRepo = portfolioRepo;
    }

    public async Task<Result<List<HoldingResponse>>> GetAllByPortfolioId(int portfolioId)
    {
        var portfolio = await _portfolioRepo.GetById(portfolioId);
        var result    = new Result<List<HoldingResponse>>();

        if (portfolio is null)
        {
            result.AddMessage("Portfolio not found.", ResultType.NotFound);
            return result;
        }

        var holdings = await _holdingRepo.GetAllByPortfolioId(portfolioId);

        decimal totalMarketValue = holdings
            .Where(h => h.Security.LastPrice.HasValue)
            .Sum(h => h.Quantity * h.Security.LastPrice!.Value);

        decimal totalPortfolioValue = portfolio.CashBalance + totalMarketValue;

        result.Payload = holdings.Select(h =>
        {
            decimal currentPrice = h.Security.LastPrice ?? 0m;
            decimal marketValue  = h.Quantity * currentPrice;
            decimal costBasis    = h.Quantity * h.AvgCost;
            decimal unrealizedGl = marketValue - costBasis;
            decimal glPct        = costBasis > 0 ? (unrealizedGl / costBasis) * 100m : 0m;
            decimal weight       = totalPortfolioValue > 0 ? (marketValue / totalPortfolioValue) * 100m : 0m;

            return new HoldingResponse
            {
                HoldingId       = h.HoldingId,
                PortfolioId     = h.PortfolioId,
                Ticker          = h.Ticker,
                CompanyName     = h.Security.CompanyName,
                AssetClass      = h.Security.AssetClass,
                Quantity        = h.Quantity,
                AvgCost         = h.AvgCost,
                CurrentPrice    = currentPrice,
                MarketValue     = marketValue,
                UnrealizedGl    = unrealizedGl,
                UnrealizedGlPct = glPct,
                PortfolioWeight = weight
            };
        }).ToList();

        return result;
    }
}