namespace Server.Data;

using Microsoft.EntityFrameworkCore;
using Server.Models;

public class EfHoldingRepository : IHoldingRepository
{
    private readonly AppDbContext _db;

    public EfHoldingRepository(AppDbContext db) => _db = db;

    public async Task<List<Holding>> GetAllByPortfolioId(int portfolioId)
    {
        return await _db.Holdings
            .AsNoTracking()
            .Include(h => h.Security)
            .Where(h => h.PortfolioId == portfolioId)
            .ToListAsync();
    }

    public async Task<Holding?> GetByPortfolioAndTicker(int portfolioId, string ticker)
    {
        return await _db.Holdings
            .FirstOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Ticker == ticker);
    }

    public async Task<Holding> Create(Holding holding)
    {
        _db.Holdings.Add(holding);
        await _db.SaveChangesAsync();
        return holding;
    }

    public async Task<Holding?> Update(Holding holding)
    {
        var existing = await _db.Holdings.FindAsync(holding.HoldingId);
        if (existing is null) return null;

        existing.Quantity  = holding.Quantity;
        existing.AvgCost   = holding.AvgCost;
        existing.UpdatedAt = holding.UpdatedAt;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> Delete(int holdingId)
    {
        var holding = await _db.Holdings.FindAsync(holdingId);
        if (holding is null) return false;

        _db.Holdings.Remove(holding);
        await _db.SaveChangesAsync();
        return true;
    }
}