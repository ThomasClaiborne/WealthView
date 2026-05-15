namespace Server.Domain;

using Server.DTOs.Requests;
using Server.DTOs.Responses;
using Server.Models;

public interface ITradeService
{
    Task<Result<List<Trade>>>   GetAll(int portfolioId);
    Task<Result<TradeResponse>> Buy(int portfolioId, BuyRequest request);
    Task<Result<TradeResponse>> Sell(int portfolioId, SellRequest request);
}