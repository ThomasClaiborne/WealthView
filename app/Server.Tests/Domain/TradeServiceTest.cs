namespace Server.Tests.Domain;

using System.Reflection;
using Moq;
using Server.Data;
using Server.Domain;
using Server.DTOs.Requests;
using Server.Models;

public class TradeServiceTest
{
    private readonly Mock<ITradeRepository>     _tradeRepo     = new();
    private readonly Mock<IHoldingRepository>   _holdingRepo   = new();
    private readonly Mock<ISecurityRepository>  _securityRepo  = new();
    private readonly Mock<IPortfolioRepository> _portfolioRepo = new();
    private readonly TradeService               _service;

    public TradeServiceTest()
    {
        _service = new TradeService(
            _tradeRepo.Object, _holdingRepo.Object,
            _securityRepo.Object, _portfolioRepo.Object);
    }

    // ── Buy ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Buy_ReturnsNotFound_WhenTickerInvalid()
    {
        _securityRepo.Setup(r => r.GetByTicker("FAKE")).ReturnsAsync((Security?)null);

        var result = await _service.Buy(1, new BuyRequest { Ticker = "FAKE", Quantity = 1 });

        Assert.Equal(ResultType.NotFound, result.Type);
        Assert.Contains("Ticker not found", result.Messages[0]);
    }

    [Fact]
    public async Task Buy_ReturnsInvalid_WhenPriceUnavailable()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = null });

        var result = await _service.Buy(1, new BuyRequest { Ticker = "AAPL", Quantity = 1 });

        Assert.Equal(ResultType.Invalid, result.Type);
        Assert.Contains("Price not yet available", result.Messages[0]);
    }

    [Fact]
    public async Task Buy_ReturnsInvalid_WhenInsufficientCash()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 100m });

        var result = await _service.Buy(1, new BuyRequest { Ticker = "AAPL", Quantity = 10 });

        Assert.Equal(ResultType.Invalid, result.Type);
        Assert.Contains("Insufficient cash", result.Messages[0]);
    }

    [Fact]
    public async Task Buy_CreatesNewHolding_WhenNoneExists()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 5000m });
        _holdingRepo.Setup(r => r.GetByPortfolioAndTicker(1, "AAPL"))
            .ReturnsAsync((Holding?)null);
        _holdingRepo.Setup(r => r.Create(It.IsAny<Holding>()))
            .ReturnsAsync((Holding h) => h);
        _portfolioRepo.Setup(r => r.UpdateCashBalance(1, It.IsAny<decimal>()))
            .ReturnsAsync(true);
        _tradeRepo.Setup(r => r.Create(It.IsAny<Trade>()))
            .ReturnsAsync((Trade t) => { t.TradeId = 1; return t; });

        var result = await _service.Buy(1, new BuyRequest { Ticker = "AAPL", Quantity = 5 });

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(4000m, result.Payload!.NewCashBalance); // 5000 - (5 × 200)
        _holdingRepo.Verify(r => r.Create(It.IsAny<Holding>()), Times.Once);
        _holdingRepo.Verify(r => r.Update(It.IsAny<Holding>()), Times.Never);
    }

    [Fact]
    public async Task Buy_UpdatesExistingHolding_AndRecalculatesAvgCost()
    {
        // Existing: 10 shares @ $150. Buying 10 more @ $200.
        // New avg cost = (10×150 + 10×200) / 20 = $175
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 5000m });
        _holdingRepo.Setup(r => r.GetByPortfolioAndTicker(1, "AAPL"))
            .ReturnsAsync(new Holding { HoldingId = 1, Ticker = "AAPL",
                                        Quantity = 10m, AvgCost = 150m });
        _holdingRepo.Setup(r => r.Update(It.IsAny<Holding>()))
            .ReturnsAsync((Holding h) => h);
        _portfolioRepo.Setup(r => r.UpdateCashBalance(1, It.IsAny<decimal>()))
            .ReturnsAsync(true);
        _tradeRepo.Setup(r => r.Create(It.IsAny<Trade>()))
            .ReturnsAsync((Trade t) => { t.TradeId = 1; return t; });

        var result = await _service.Buy(1, new BuyRequest { Ticker = "AAPL", Quantity = 10 });

        Assert.Equal(ResultType.Success, result.Type);
        _holdingRepo.Verify(r => r.Update(It.Is<Holding>(
            h => h.Quantity == 20m && h.AvgCost == 175m)), Times.Once);
        _holdingRepo.Verify(r => r.Create(It.IsAny<Holding>()), Times.Never);
    }

    // ── Sell ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Sell_ReturnsNotFound_WhenTickerInvalid()
    {
        _securityRepo.Setup(r => r.GetByTicker("FAKE")).ReturnsAsync((Security?)null);

        var result = await _service.Sell(1, new SellRequest { Ticker = "FAKE", Quantity = 1 });

        Assert.Equal(ResultType.NotFound, result.Type);
    }

    [Fact]
    public async Task Sell_ReturnsInvalid_WhenHoldingNotFound()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _holdingRepo.Setup(r => r.GetByPortfolioAndTicker(1, "AAPL"))
            .ReturnsAsync((Holding?)null);

        var result = await _service.Sell(1, new SellRequest { Ticker = "AAPL", Quantity = 1 });

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task Sell_ReturnsInvalid_WhenInsufficientShares()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _holdingRepo.Setup(r => r.GetByPortfolioAndTicker(1, "AAPL"))
            .ReturnsAsync(new Holding { Quantity = 5m });

        var result = await _service.Sell(1, new SellRequest { Ticker = "AAPL", Quantity = 10 });

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task Sell_DeletesHolding_WhenFullySold()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _holdingRepo.Setup(r => r.GetByPortfolioAndTicker(1, "AAPL"))
            .ReturnsAsync(new Holding { HoldingId = 1, Ticker = "AAPL", Quantity = 10m });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 1000m });
        _portfolioRepo.Setup(r => r.UpdateCashBalance(1, It.IsAny<decimal>()))
            .ReturnsAsync(true);
        _holdingRepo.Setup(r => r.Delete(1)).ReturnsAsync(true);
        _tradeRepo.Setup(r => r.Create(It.IsAny<Trade>()))
            .ReturnsAsync((Trade t) => { t.TradeId = 1; return t; });

        var result = await _service.Sell(1, new SellRequest { Ticker = "AAPL", Quantity = 10 });

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(3000m, result.Payload!.NewCashBalance); 
        _holdingRepo.Verify(r => r.Delete(1), Times.Once);
        _holdingRepo.Verify(r => r.Update(It.IsAny<Holding>()), Times.Never);
    }

    [Fact]
    public async Task Sell_ReducesHolding_WhenPartiallySold()
    {
        _securityRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", LastPrice = 200m });
        _holdingRepo.Setup(r => r.GetByPortfolioAndTicker(1, "AAPL"))
            .ReturnsAsync(new Holding { HoldingId = 1, Ticker = "AAPL", Quantity = 10m });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 1000m });
        _portfolioRepo.Setup(r => r.UpdateCashBalance(1, It.IsAny<decimal>()))
            .ReturnsAsync(true);
        _holdingRepo.Setup(r => r.Update(It.IsAny<Holding>()))
            .ReturnsAsync((Holding h) => h);
        _tradeRepo.Setup(r => r.Create(It.IsAny<Trade>()))
            .ReturnsAsync((Trade t) => { t.TradeId = 1; return t; });

        var result = await _service.Sell(1, new SellRequest { Ticker = "AAPL", Quantity = 4 });

        Assert.Equal(ResultType.Success, result.Type);
        _holdingRepo.Verify(r => r.Update(It.Is<Holding>(
            h => h.Quantity == 6m)), Times.Once);
        _holdingRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }
}