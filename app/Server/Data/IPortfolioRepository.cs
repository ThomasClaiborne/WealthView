using Server.Models;

namespace Server.Data;

public interface IPortfolioRepository
{
    Task<Portfolio> Create(Portfolio portfolio);
    Task<Portfolio?> GetByUserId(int userId);
    Task<Portfolio?> GetById(int portfolioId);
    Task<bool> UpdateCashBalance(int portfolioId, decimal newBalance);
    Task<List<PortfolioSnapshot>> GetSnapshotsByPortfolioId(int portfolioId, int days = 30);
    Task<bool> SnapshotExistsForToday(int portfolioId);
    Task<PortfolioSnapshot> CreateSnapshot(PortfolioSnapshot snapshot);
}