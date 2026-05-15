using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Tests.Data;

[Collection("DatabaseTests")]
public class EfHoldingRepositoryTest : IAsyncLifetime
{
    private readonly EfHoldingRepository _repo;
    private readonly AppDbContext _db;

    public EfHoldingRepositoryTest()
    {
        _db   = TestDbContextFactory.Create();
        _repo = new EfHoldingRepository(_db);
    }

    public async Task InitializeAsync() =>
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");

    public Task DisposeAsync() => Task.CompletedTask;

    // GetAllByPortfolioId
    [Fact]
    public async Task GetAllByPortfolioId_ReturnsHoldings_WhenPortfolioHasHoldings()
    {
        var result = await _repo.GetAllByPortfolioId(1);
        Assert.Single(result);
        Assert.Equal("AAPL", result[0].Ticker);
    }

    [Fact]
    public async Task GetAllByPortfolioId_ReturnsEmpty_WhenNoHoldings()
    {
        var result = await _repo.GetAllByPortfolioId(999);
        Assert.Empty(result);
    }

    // GetByPortfolioAndTicker
    [Fact]
    public async Task GetByPortfolioAndTicker_ReturnsHolding_WhenExists()
    {
        var result = await _repo.GetByPortfolioAndTicker(1, "AAPL");
        Assert.NotNull(result);
        Assert.Equal(10.0000m, result.Quantity);
        Assert.Equal(165.0000m, result.AvgCost);
    }

    [Fact]
    public async Task GetByPortfolioAndTicker_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByPortfolioAndTicker(1, "FAKE");
        Assert.Null(result);
    }

    // Create
    [Fact]
    public async Task Create_ReturnsHolding_WithId()
    {
        var holding = new Holding
        {
            PortfolioId = 1,
            Ticker      = "MSFT",
            Quantity    = 5.0000m,
            AvgCost     = 300.0000m,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        var result = await _repo.Create(holding);

        Assert.True(result.HoldingId > 0);
        Assert.Equal("MSFT", result.Ticker);
    }

    // Update
    [Fact]
    public async Task Update_ReturnsUpdatedHolding_WhenExists()
    {
        var holding = await _repo.GetByPortfolioAndTicker(1, "AAPL");
        holding!.Quantity  = 15.0000m;
        holding.AvgCost    = 170.0000m;
        holding.UpdatedAt  = DateTime.UtcNow;

        var result = await _repo.Update(holding);

        Assert.NotNull(result);
        Assert.Equal(15.0000m, result.Quantity);
        Assert.Equal(170.0000m, result.AvgCost);
    }

    [Fact]
    public async Task Update_ReturnsNull_WhenNotFound()
    {
        var ghost = new Holding { HoldingId = 999, Quantity = 1, AvgCost = 1,
                                  UpdatedAt = DateTime.UtcNow };
        var result = await _repo.Update(ghost);
        Assert.Null(result);
    }

    // Delete
    [Fact]
    public async Task Delete_ReturnsTrue_WhenExists()
    {
        var result = await _repo.Delete(1);
        Assert.True(result);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_WhenNotFound()
    {
        var result = await _repo.Delete(999);
        Assert.False(result);
    }
}