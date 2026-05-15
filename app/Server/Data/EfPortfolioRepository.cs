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
}