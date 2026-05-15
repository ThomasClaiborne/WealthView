namespace Server.Domain;

using Server.DTOs.Responses;
using Server.Models;

public interface IHoldingService
{
    Task<Result<List<HoldingResponse>>> GetAllByPortfolioId(int portfolioId);
}