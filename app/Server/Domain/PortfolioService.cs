// Replace the entire PortfolioService.cs with this:

using Server.Data;
using Server.DTOs.Responses;
using Server.Models;

namespace Server.Domain;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IHoldingRepository   _holdingRepo;

    public PortfolioService(IPortfolioRepository portfolioRepo, IHoldingRepository holdingRepo)
    {
        _portfolioRepo = portfolioRepo;
        _holdingRepo   = holdingRepo;
    }

    public async Task<Result<PortfolioResponse>> GetByUserId(int userId)
    {
        var result    = new Result<PortfolioResponse>();
        var portfolio = await _portfolioRepo.GetByUserId(userId);

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

        decimal totalValue = portfolio.CashBalance + totalMarketValue;

        await EnsureTodaySnapshot(portfolio.PortfolioId, totalValue);

        result.Payload = new PortfolioResponse
        {
            PortfolioId       = portfolio.PortfolioId,
            CashBalance       = portfolio.CashBalance,
            TotalValue        = totalValue,
            TotalUnrealizedGl = totalUnrealizedGl,
            HoldingCount      = holdings.Count
        };

        return result;
    }

    public async Task<Result<List<SnapshotResponse>>> GetSnapshotHistory(int portfolioId)
    {
        var result    = new Result<List<SnapshotResponse>>();
        var snapshots = await _portfolioRepo.GetSnapshotsByPortfolioId(portfolioId);

        result.Payload = snapshots.Select(s => new SnapshotResponse
        {
            SnapshotId   = s.SnapshotId,
            SnapshotDate = s.SnapshotDate,
            TotalValue   = s.TotalValue
        }).ToList();

        return result;
    }

    private async Task EnsureTodaySnapshot(int portfolioId, decimal totalValue)
    {
        var exists = await _portfolioRepo.SnapshotExistsForToday(portfolioId);
        if (exists) return;

        await _portfolioRepo.CreateSnapshot(new PortfolioSnapshot
        {
            PortfolioId  = portfolioId,
            SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalValue   = totalValue
        });
    }
}