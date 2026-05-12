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
}