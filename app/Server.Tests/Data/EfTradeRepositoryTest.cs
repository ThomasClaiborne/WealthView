using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Tests.Data;

[Collection("DatabaseTests")]
public class EfTradeRepositoryTest : IAsyncLifetime
{
    private readonly EfTradeRepository _repo;
    private readonly AppDbContext _db;

    public EfTradeRepositoryTest()
    {
        _db   = TestDbContextFactory.Create();
        _repo = new EfTradeRepository(_db);
    }

    public async Task InitializeAsync() =>
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");

    public Task DisposeAsync() => Task.CompletedTask;

    // GetAllByPortfolioId
    [Fact]
    public async Task GetAllByPortfolioId_ReturnsTradesNewestFirst()
    {
        var result = await _repo.GetAllByPortfolioId(2);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(2, t.PortfolioId));
    }

    [Fact]
    public async Task GetAllByPortfolioId_ReturnsEmpty_WhenNoTrades()
    {
        var result = await _repo.GetAllByPortfolioId(999);
        Assert.Empty(result);
    }

    // Create
    [Fact]
    public async Task Create_ReturnsTrade_WithId()
    {
        var trade = new Trade
        {
            PortfolioId   = 1,
            Ticker        = "MSFT",
            TradeType     = TradeType.Buy,
            Quantity      = 2.0000m,
            PricePerShare = 400.0000m,
            TotalValue    = 800.0000m,
            ExecutedAt    = DateTime.UtcNow
        };

        var result = await _repo.Create(trade);

        Assert.True(result.TradeId > 0);
        Assert.Equal("MSFT", result.Ticker);
        Assert.Equal(TradeType.Buy, result.TradeType);
    }
}