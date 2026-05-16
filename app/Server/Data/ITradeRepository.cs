namespace Server.Data;

using Server.Models;

public interface ITradeRepository
{
    Task<List<Trade>> GetAllByPortfolioId(int portfolioId);
    Task<Trade>       Create(Trade trade);
}