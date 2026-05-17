using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class EfPortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext _db;

    public EfPortfolioRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Portfolio> Create(Portfolio portfolio)
    {
        _db.Portfolios.Add(portfolio);
        await _db.SaveChangesAsync();
        return portfolio;
    }

    public async Task<Portfolio?> GetByUserId(int userId)
    {
        return await _db.Portfolios
            .FirstOrDefaultAsync(p => p.AppUserId == userId);
    }

    public async Task<Portfolio?> GetById(int portfolioId)
    {
        return await _db.Portfolios
            .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);
    }

    public async Task<bool> UpdateCashBalance(int portfolioId, decimal newBalance)
    {
        var portfolio = await _db.Portfolios.FindAsync(portfolioId);
        if (portfolio == null) return false;

        portfolio.CashBalance = newBalance;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PortfolioSnapshot>> GetSnapshotsByPortfolioId(int portfolioId, int days = 30)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        return await _db.PortfolioSnapshots
            .AsNoTracking()
            .Where(s => s.PortfolioId == portfolioId && s.SnapshotDate >= cutoff)
            .OrderBy(s => s.SnapshotDate)
            .ToListAsync();
    }

    public async Task<bool> SnapshotExistsForToday(int portfolioId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _db.PortfolioSnapshots
            .AnyAsync(s => s.PortfolioId == portfolioId && s.SnapshotDate == today);
    }

    public async Task<PortfolioSnapshot> CreateSnapshot(PortfolioSnapshot snapshot)
    {
        _db.PortfolioSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();
        return snapshot;
    }
}