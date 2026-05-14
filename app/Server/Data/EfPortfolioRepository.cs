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
}