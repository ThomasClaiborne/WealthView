using Server.Models;

namespace Server.Data;

public interface IPortfolioRepository
{
    Task<Portfolio> Create(Portfolio portfolio);
    Task<Portfolio?> GetByUserId(int userId);
    Task<Portfolio?> GetById(int portfolioId);
    Task<bool> UpdateCashBalance(int portfolioId, decimal newBalance);
}