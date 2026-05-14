using Server.DTOs.Responses;

namespace Server.Domain;

public interface IPortfolioService
{
    Task<Result<PortfolioResponse>> GetByUserId(int userId);
}