using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Tests;

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
}