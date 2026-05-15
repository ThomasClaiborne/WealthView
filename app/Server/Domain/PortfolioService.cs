using Server.Data;
using Server.DTOs.Responses;

namespace Server.Domain;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IHoldingRepository _holdingRepo;

    public PortfolioService(IPortfolioRepository portfolioRepo, IHoldingRepository holdingRepo)
    {
        _portfolioRepo = portfolioRepo;
        _holdingRepo = holdingRepo;
    }

    public async Task<Result<PortfolioResponse>> GetByUserId(int userId)
    {
        var portfolio = await _portfolioRepo.GetByUserId(userId);
        var result = new Result<PortfolioResponse>();

        if (portfolio == null)
        {
            result.AddMessage("Portfolio not found.", ResultType.NotFound);
            return result;
        }

        var holdings = await _holdingRepo.GetAllByPortfolioId(portfolio.PortfolioId);

        decimal totalMarketValue = holdings
            .Where(h => h.Security.LastPrice.HasValue)
            .Sum(h => h.Quantity * h.Security.LastPrice!.Value);

        decimal totalUnrealizedGl = holdings
            .Where(h => h.Security.LastPrice.HasValue)
            .Sum(h => (h.Security.LastPrice!.Value - h.AvgCost) * h.Quantity);

        result.Payload = new PortfolioResponse
        {
            PortfolioId = portfolio.PortfolioId,
            CashBalance = portfolio.CashBalance,
            TotalValue = portfolio.CashBalance + totalMarketValue,
            TotalUnrealizedGl = totalUnrealizedGl,
            HoldingCount = holdings.Count
        };

        return result;
    }
}