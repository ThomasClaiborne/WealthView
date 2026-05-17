using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Tests.Data;

[Collection("DatabaseTests")]
public class EfPortfolioRepositoryTest : IAsyncLifetime
{
    private AppDbContext _db = null!;
    private EfPortfolioRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.Create();
        _repo = new EfPortfolioRepository(_db);
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Create_InsertsPortfolio_AndReturnsWithGeneratedId()
    {
        // KnownGoodState has users 1 and 2 with portfolios already.
        // Insert a third user first so we have a valid FK target.
        var userRepo = new EfUserRepository(_db);
        var newUser = await userRepo.Create(new AppUser
        {
            Username = "portfoliotest",
            Email = "portfolio@wealthview.com",
            PasswordHash = "hashedpassword",
            FirstName = "Portfolio",
            LastName = "Test",
            CreatedAt = DateTime.UtcNow
        });

        var newPortfolio = new Portfolio
        {
            AppUserId = newUser.AppUserId,
            CashBalance = 0,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repo.Create(newPortfolio);

        Assert.True(result.PortfolioId > 0);
        Assert.Equal(newUser.AppUserId, result.AppUserId);
        Assert.Equal(0, result.CashBalance);
    }

    // ── GetByUserId ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserId_ReturnsPortfolio_WhenUserExists()
    {
        // KnownGoodState seeds portfolio for userId = 1 (jdoe) with cash_balance = 850
        var result = await _repo.GetByUserId(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.AppUserId);
        Assert.Equal(850.00m, result.CashBalance);
    }

    [Fact]
    public async Task GetByUserId_ReturnsNull_WhenUserHasNoPortfolio()
    {
        var result = await _repo.GetByUserId(999);

        Assert.Null(result);
    }

    // ── GetSnapshotsByPortfolioId ─────────────────────────────────────────────

    [Fact]
    public async Task GetSnapshotsByPortfolioId_ReturnsSnapshots_OrderedByDateAscending()
    {
        // known good state seeds snapshots for portfolio 1: Jan 2 and Jan 3
        var result = await _repo.GetSnapshotsByPortfolioId(1);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].SnapshotDate < result[1].SnapshotDate);
    }

    [Fact]
    public async Task GetSnapshotsByPortfolioId_ReturnsEmpty_WhenNoSnapshots()
    {
        var result = await _repo.GetSnapshotsByPortfolioId(999);

        Assert.Empty(result);
    }

    // ── SnapshotExistsForToday ────────────────────────────────────────────────

    [Fact]
    public async Task SnapshotExistsForToday_ReturnsFalse_WhenNoSnapshotForToday()
    {
        // known good state seeds snapshots for Jan 2 and Jan 3 — not today
        var result = await _repo.SnapshotExistsForToday(1);

        Assert.False(result);
    }

    [Fact]
    public async Task SnapshotExistsForToday_ReturnsTrue_WhenSnapshotExistsForToday()
    {
        // insert a snapshot for today first
        await _repo.CreateSnapshot(new PortfolioSnapshot
        {
            PortfolioId = 1,
            SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalValue = 9999m
        });

        var result = await _repo.SnapshotExistsForToday(1);

        Assert.True(result);
    }

    // ── CreateSnapshot ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSnapshot_ReturnsSnapshot_WithId()
    {
        var snapshot = new PortfolioSnapshot
        {
            PortfolioId = 1,
            SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalValue = 5000m
        };

        var result = await _repo.CreateSnapshot(snapshot);

        Assert.True(result.SnapshotId > 0);
        Assert.Equal(5000m, result.TotalValue);
    }
}