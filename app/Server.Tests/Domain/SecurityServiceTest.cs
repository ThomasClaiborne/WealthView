using Moq;
using Server.Data;
using Server.Domain;
using Server.Models;

namespace Server.Tests.Domain;

[Collection("DatabaseTests")]
public class SecurityServiceTest
{
    private static List<Security> SeedSecurities(decimal? lastPrice = null, DateTime? fetchedAt = null) =>
    [
        new Security { Ticker = "AAPL", CompanyName = "Apple Inc.",       AssetClass = AssetClass.Equity, LastPrice = lastPrice, PriceFetchedAt = fetchedAt },
        new Security { Ticker = "SPY",  CompanyName = "SPDR S&P 500 ETF", AssetClass = AssetClass.ETF,    LastPrice = lastPrice, PriceFetchedAt = fetchedAt }
    ];

    // ── GetAll ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsSuccess_WithPricesSet_WhenPricesWereNull()
    {
        var mockRepo   = new Mock<ISecurityRepository>();
        var mockMarket = new Mock<IMarketDataService>();

        mockRepo.Setup(r => r.GetAll()).ReturnsAsync(SeedSecurities());
        mockMarket.Setup(m => m.GetLivePrice("AAPL")).ReturnsAsync(185.50m);
        mockMarket.Setup(m => m.GetLivePrice("SPY" )).ReturnsAsync(450.00m);

        var service = new SecurityService(mockRepo.Object, mockMarket.Object);
        var result  = await service.GetAll();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Payload!.Count);

        var aapl = result.Payload.First(s => s.Ticker == "AAPL");
        Assert.Equal(185.50m, aapl.LastPrice);
    }

    [Fact]
    public async Task GetAll_CallsMarketData_ForEachSecurityNeedingRefresh()
    {
        var mockRepo   = new Mock<ISecurityRepository>();
        var mockMarket = new Mock<IMarketDataService>();

        mockRepo.Setup(r => r.GetAll()).ReturnsAsync(SeedSecurities());
        mockMarket.Setup(m => m.GetLivePrice(It.IsAny<string>())).ReturnsAsync(100.00m);

        var service = new SecurityService(mockRepo.Object, mockMarket.Object);
        await service.GetAll();

        mockMarket.Verify(m => m.GetLivePrice("AAPL"), Times.Once);
        mockMarket.Verify(m => m.GetLivePrice("SPY"),  Times.Once);
    }

    [Fact]
    public async Task GetAll_SkipsPriceFetch_WhenPriceAlreadyFetchedToday()
    {
        var mockRepo   = new Mock<ISecurityRepository>();
        var mockMarket = new Mock<IMarketDataService>();

        // Securities already have a price fetched today
        mockRepo.Setup(r => r.GetAll())
            .ReturnsAsync(SeedSecurities(lastPrice: 180.00m, fetchedAt: DateTime.UtcNow));

        var service = new SecurityService(mockRepo.Object, mockMarket.Object);
        var result  = await service.GetAll();

        Assert.True(result.IsSuccess);

        // API should never be called — prices are fresh
        mockMarket.Verify(m => m.GetLivePrice(It.IsAny<string>()), Times.Never);
        Assert.Equal(180.00m, result.Payload![0].LastPrice);
    }

    [Fact]
    public async Task GetAll_ReturnsSuccess_WithNullPrice_WhenMarketDataFails()
    {
        var mockRepo   = new Mock<ISecurityRepository>();
        var mockMarket = new Mock<IMarketDataService>();

        mockRepo.Setup(r => r.GetAll()).ReturnsAsync(SeedSecurities());
        mockMarket.Setup(m => m.GetLivePrice(It.IsAny<string>())).ReturnsAsync((decimal?)null);

        var service = new SecurityService(mockRepo.Object, mockMarket.Object);
        var result  = await service.GetAll();

        // Still succeeds — market data failure is graceful
        Assert.True(result.IsSuccess);
        Assert.Null(result.Payload![0].LastPrice);
    }

    // ── GetByTicker ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByTicker_ReturnsSecurity_WhenTickerExists()
    {
        var mockRepo   = new Mock<ISecurityRepository>();
        var mockMarket = new Mock<IMarketDataService>();

        mockRepo.Setup(r => r.GetByTicker("AAPL"))
            .ReturnsAsync(new Security { Ticker = "AAPL", CompanyName = "Apple Inc.", AssetClass = AssetClass.Equity });

        var service = new SecurityService(mockRepo.Object, mockMarket.Object);
        var result  = await service.GetByTicker("AAPL");

        Assert.True(result.IsSuccess);
        Assert.Equal("AAPL", result.Payload!.Ticker);
    }

    [Fact]
    public async Task GetByTicker_ReturnsNotFound_WhenTickerDoesNotExist()
    {
        var mockRepo   = new Mock<ISecurityRepository>();
        var mockMarket = new Mock<IMarketDataService>();

        mockRepo.Setup(r => r.GetByTicker("FAKE")).ReturnsAsync((Security?)null);

        var service = new SecurityService(mockRepo.Object, mockMarket.Object);
        var result  = await service.GetByTicker("FAKE");

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.NotFound, result.Type);
    }
}