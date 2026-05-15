namespace Server.Tests.Domain;

using Moq;
using Server.Data;
using Server.Domain;
using Server.DTOs.Responses;
using Server.Models;

public class HoldingServiceTest
{
    private readonly Mock<IHoldingRepository>   _holdingRepo  = new();
    private readonly Mock<IPortfolioRepository> _portfolioRepo = new();
    private readonly HoldingService             _service;

    public HoldingServiceTest()
    {
        _service = new HoldingService(_holdingRepo.Object, _portfolioRepo.Object);
    }

    [Fact]
    public async Task GetAllByPortfolioId_ReturnsNotFound_WhenPortfolioMissing()
    {
        _portfolioRepo.Setup(r => r.GetById(99)).ReturnsAsync((Portfolio?)null);

        var result = await _service.GetAllByPortfolioId(99);

        Assert.Equal(ResultType.NotFound, result.Type);
    }

    [Fact]
    public async Task GetAllByPortfolioId_ReturnsEmptyList_WhenNoHoldings()
    {
        _portfolioRepo.Setup(r => r.GetById(1)).ReturnsAsync(new Portfolio
            { PortfolioId = 1, CashBalance = 1000m });
        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1)).ReturnsAsync(new List<Holding>());

        var result = await _service.GetAllByPortfolioId(1);

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Empty(result.Payload!);
    }

    [Fact]
    public async Task GetAllByPortfolioId_ComputesMarketValueAndUnrealizedGl()
    {
        _portfolioRepo.Setup(r => r.GetById(1)).ReturnsAsync(new Portfolio
            { PortfolioId = 1, CashBalance = 1000m });

        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1)).ReturnsAsync(new List<Holding>
        {
            new()
            {
                HoldingId   = 1, PortfolioId = 1,
                Ticker      = "AAPL",
                Quantity    = 10m, AvgCost = 150m,
                Security    = new Security
                {
                    Ticker      = "AAPL",
                    CompanyName = "Apple Inc.",
                    AssetClass  = AssetClass.Equity,
                    LastPrice   = 200m
                }
            }
        });

        var result = await _service.GetAllByPortfolioId(1);
        var h      = result.Payload![0];

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(2000m,  h.MarketValue);      // 10 × 200
        Assert.Equal(500m,   h.UnrealizedGl);     // (200 - 150) × 10
        Assert.Equal(33.33m, Math.Round(h.UnrealizedGlPct, 2)); // 500/1500 × 100
    }

    [Fact]
    public async Task GetAllByPortfolioId_ComputesPortfolioWeight()
    {
        // Cash = 1000, MarketValue = 2000 → TotalPortfolioValue = 3000
        // Weight = 2000/3000 × 100 = 66.67%
        _portfolioRepo.Setup(r => r.GetById(1)).ReturnsAsync(new Portfolio
            { PortfolioId = 1, CashBalance = 1000m });

        _holdingRepo.Setup(r => r.GetAllByPortfolioId(1)).ReturnsAsync(new List<Holding>
        {
            new()
            {
                HoldingId = 1, PortfolioId = 1, Ticker = "AAPL",
                Quantity  = 10m, AvgCost = 150m,
                Security  = new Security
                    { Ticker = "AAPL", CompanyName = "Apple Inc.",
                      AssetClass = AssetClass.Equity, LastPrice = 200m }
            }
        });

        var result = await _service.GetAllByPortfolioId(1);
        var h      = result.Payload![0];

        Assert.Equal(66.67m, Math.Round(h.PortfolioWeight, 2));
    }
}