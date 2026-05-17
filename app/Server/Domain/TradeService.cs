namespace Server.Domain;

using Server.Data;
using Server.DTOs.Requests;
using Server.DTOs.Responses;
using Server.Models;

public class TradeService : ITradeService
{
    private readonly ITradeRepository     _tradeRepo;
    private readonly IHoldingRepository   _holdingRepo;
    private readonly ISecurityRepository  _securityRepo;
    private readonly IPortfolioRepository _portfolioRepo;

    public TradeService(
        ITradeRepository     tradeRepo,
        IHoldingRepository   holdingRepo,
        ISecurityRepository  securityRepo,
        IPortfolioRepository portfolioRepo)
    {
        _tradeRepo     = tradeRepo;
        _holdingRepo   = holdingRepo;
        _securityRepo  = securityRepo;
        _portfolioRepo = portfolioRepo;
    }

    public async Task<Result<List<Trade>>> GetAll(int portfolioId)
    {
        var result   = new Result<List<Trade>>();
        result.Payload = await _tradeRepo.GetAllByPortfolioId(portfolioId);
        return result;
    }

    public async Task<Result<TradeResponse>> Buy(int portfolioId, BuyRequest request)
    {
        var result = new Result<TradeResponse>();

        var security = await _securityRepo.GetByTicker(request.Ticker);
        if (security is null)
        {
            result.AddMessage("Ticker not found.", ResultType.NotFound);
            return result;
        }

        if (security.LastPrice is null)
        {
            result.AddMessage("Price not yet available for this ticker.", ResultType.Invalid);
            return result;
        }

        decimal price     = security.LastPrice.Value;
        decimal totalCost = request.Quantity * price;

        var portfolio = await _portfolioRepo.GetById(portfolioId);
        if (portfolio!.CashBalance < totalCost)
        {
            result.AddMessage("Insufficient cash balance.", ResultType.Invalid);
            return result;
        }

        var now = DateTime.UtcNow;

        decimal newCashBalance = portfolio.CashBalance - totalCost;
        await _portfolioRepo.UpdateCashBalance(portfolioId, newCashBalance);

        var existing = await _holdingRepo.GetByPortfolioAndTicker(portfolioId, request.Ticker);
        if (existing is null)
        {
            await _holdingRepo.Create(new Holding
            {
                PortfolioId = portfolioId,
                Ticker      = request.Ticker,
                Quantity    = request.Quantity,
                AvgCost     = price,
                CreatedAt   = now,
                UpdatedAt   = now
            });
        }
        else
        {
            decimal newQty     = existing.Quantity + request.Quantity;
            decimal newAvgCost = (existing.Quantity * existing.AvgCost + request.Quantity * price) / newQty;

            existing.Quantity  = newQty;
            existing.AvgCost   = newAvgCost;
            existing.UpdatedAt = now;
            await _holdingRepo.Update(existing);
        }

        var trade = await _tradeRepo.Create(new Trade
        {
            PortfolioId   = portfolioId,
            Ticker        = request.Ticker,
            TradeType     = TradeType.Buy,
            Quantity      = request.Quantity,
            PricePerShare = price,
            TotalValue    = totalCost,
            ExecutedAt    = now
        });

        result.Payload = ToResponse(trade, newCashBalance);
        return result;
    }

    public async Task<Result<TradeResponse>> Sell(int portfolioId, SellRequest request)
    {
        var result = new Result<TradeResponse>();

        var security = await _securityRepo.GetByTicker(request.Ticker);
        if (security is null)
        {
            result.AddMessage("Ticker not found.", ResultType.NotFound);
            return result;
        }

        if (security.LastPrice is null)
        {
            result.AddMessage("Price not yet available for this ticker.", ResultType.Invalid);
            return result;
        }

        decimal price    = security.LastPrice.Value;
        decimal proceeds = request.Quantity * price;

        var holding = await _holdingRepo.GetByPortfolioAndTicker(portfolioId, request.Ticker);
        if (holding is null)
        {
            result.AddMessage("You do not own this security.", ResultType.Invalid);
            return result;
        }

        if (holding.Quantity < request.Quantity)
        {
            result.AddMessage("Insufficient shares.", ResultType.Invalid);
            return result;
        }

        var portfolio = await _portfolioRepo.GetById(portfolioId);
        var now       = DateTime.UtcNow;

        decimal newCashBalance = portfolio!.CashBalance + proceeds;
        await _portfolioRepo.UpdateCashBalance(portfolioId, newCashBalance);

        if (holding.Quantity == request.Quantity)
        {
            await _holdingRepo.Delete(holding.HoldingId);
        }
        else
        {
            holding.Quantity  -= request.Quantity;
            holding.UpdatedAt  = now;
            await _holdingRepo.Update(holding);
        }

        var trade = await _tradeRepo.Create(new Trade
        {
            PortfolioId   = portfolioId,
            Ticker        = request.Ticker,
            TradeType     = TradeType.Sell,
            Quantity      = request.Quantity,
            PricePerShare = price,
            TotalValue    = proceeds,
            ExecutedAt    = now
        });

        result.Payload = ToResponse(trade, newCashBalance);
        return result;
    }

    private static TradeResponse ToResponse(Trade trade, decimal newCashBalance) => new()
    {
        TradeId        = trade.TradeId,
        Ticker         = trade.Ticker,
        TradeType      = trade.TradeType,
        Quantity       = trade.Quantity,
        PricePerShare  = trade.PricePerShare,
        TotalValue     = trade.TotalValue,
        ExecutedAt     = trade.ExecutedAt,
        NewCashBalance = newCashBalance
    };
}