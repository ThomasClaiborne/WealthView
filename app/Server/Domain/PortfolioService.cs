using Server.Data;
using Server.DTOs.Responses;

namespace Server.Domain;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepo;

    public PortfolioService(IPortfolioRepository portfolioRepo)
    {
        _portfolioRepo = portfolioRepo;
    }

    public async Task<Result<PortfolioResponse>> GetByUserId(int userId)
    {
        var portfolio = await _portfolioRepo.GetByUserId(userId);
        var result    = new Result<PortfolioResponse>();

        if (portfolio == null)
        {
            result.AddMessage("Portfolio not found.", ResultType.NotFound);
            return result;
        }

        // Slice 3 will compute TotalValue and TotalUnrealizedGl from holdings + live prices
        // For now: TotalValue = CashBalance, no holdings exist yet
        result.Payload = new PortfolioResponse
        {
            PortfolioId      = portfolio.PortfolioId,
            CashBalance      = portfolio.CashBalance,
            TotalValue       = portfolio.CashBalance,
            TotalUnrealizedGl = 0,
            HoldingCount     = 0
        };
        return result;
    }
}