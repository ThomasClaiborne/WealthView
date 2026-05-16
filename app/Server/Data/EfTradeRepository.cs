namespace Server.Data;

using Microsoft.EntityFrameworkCore;
using Server.Models;

public class EfTradeRepository : ITradeRepository
{
    private readonly AppDbContext _db;

    public EfTradeRepository(AppDbContext db) => _db = db;

    public async Task<List<Trade>> GetAllByPortfolioId(int portfolioId)
    {
        return await _db.Trades
            .AsNoTracking()
            .Where(t => t.PortfolioId == portfolioId)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync();
    }

    public async Task<Trade> Create(Trade trade)
    {
        _db.Trades.Add(trade);
        await _db.SaveChangesAsync();
        return trade;
    }
}