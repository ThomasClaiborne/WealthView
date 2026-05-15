namespace Server.Data;

using Server.Models;

public interface IHoldingRepository
{
    Task<List<Holding>> GetAllByPortfolioId(int portfolioId);
    Task<Holding?> GetByPortfolioAndTicker(int portfolioId, string ticker);
    Task<Holding> Create(Holding holding);
    Task<Holding?> Update(Holding holding);
    Task<bool> Delete(int holdingId);
}