using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Tests.Data;

[Collection("DatabaseTests")]
public class EfSecurityRepositoryTest : IAsyncLifetime
{
    private AppDbContext _db = null!;
    private EfSecurityRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _db   = TestDbContextFactory.Create();
        _repo = new EfSecurityRepository(_db);
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    // ── GetAll ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsList_WithAllNineSecurities()
    {
        var result = await _repo.GetAll();

        Assert.NotNull(result);
        Assert.Equal(9, result.Count);
    }

    [Fact]
    public async Task GetAll_ReturnsList_OrderedByTickerAscending()
    {
        var result = await _repo.GetAll();

        Assert.Equal("AAPL", result[0].Ticker);
        Assert.Equal("TSLA", result[8].Ticker);
    }

    // ── GetByTicker ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByTicker_ReturnsSecurity_WhenTickerExists()
    {
        var result = await _repo.GetByTicker("AAPL");

        Assert.NotNull(result);
        Assert.Equal("AAPL",       result.Ticker);
        Assert.Equal("Apple Inc.", result.CompanyName);
    }

    [Fact]
    public async Task GetByTicker_ReturnsNull_WhenTickerNotFound()
    {
        var result = await _repo.GetByTicker("FAKE");

        Assert.Null(result);
    }

    // ── UpdatePrice ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePrice_ReturnsTrue_AndPersistsPrice_WhenTickerExists()
    {
        var result = await _repo.UpdatePrice("AAPL", 185.50m);

        // ExecuteUpdateAsync bypasses the change tracker
        // so GetByTicker queries fresh from the DB
        var updated = await _repo.GetByTicker("AAPL");

        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal(185.50m, updated.LastPrice);
        Assert.NotNull(updated.PriceFetchedAt);
    }

    [Fact]
    public async Task UpdatePrice_ReturnsFalse_WhenTickerNotFound()
    {
        var result = await _repo.UpdatePrice("FAKE", 100.00m);

        Assert.False(result);
    }
}