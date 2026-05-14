using Server.Models;

namespace Server.Domain;

public interface ISecurityService
{
    Task<Result<List<Security>>> GetAll();
    Task<Result<Security>> GetByTicker(string ticker);
}