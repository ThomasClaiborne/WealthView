using Server.Models;

namespace Server.Data;

public interface IPortfolioRepository
{
    Task<Portfolio> Create(Portfolio portfolio);
    Task<Portfolio?> GetByUserId(int userId);
}