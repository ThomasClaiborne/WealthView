using Server.Models;

namespace Server.Data;

public interface ISecurityRepository
{
    Task<List<Security>> GetAll();
    Task<Security?> GetByTicker(string ticker);
    Task<bool> UpdatePrice(string ticker, decimal price);
} 