using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class EfSecurityRepository : ISecurityRepository
{
    private readonly AppDbContext _db;
    public EfSecurityRepository(AppDbContext db) => _db = db;

    public async Task<List<Security>> GetAll()
    {
        return await _db.Securities
            .OrderBy(s => s.Ticker)
            .ToListAsync();
    }

    public async Task<Security?> GetByTicker(string ticker)
    {
        return await _db.Securities
            .FirstOrDefaultAsync(s => s.Ticker == ticker);
    }

    public async Task<bool> UpdatePrice(string ticker, decimal price)
    {
        var rowsAffected = await _db.Securities
            .Where(s => s.Ticker == ticker)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastPrice,      price)
                .SetProperty(x => x.PriceFetchedAt, DateTime.UtcNow));

        return rowsAffected > 0;
    }
}