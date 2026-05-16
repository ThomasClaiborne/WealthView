namespace Server.Tests.Data;

using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

[Collection("DatabaseTests")]
public class EfBankAccountRepositoryTest : IAsyncLifetime
{
    private readonly AppDbContext          _db;
    private readonly EfBankAccountRepository _repo;

    public EfBankAccountRepositoryTest()
    {
        _db   = TestDbContextFactory.Create();
        _repo = new EfBankAccountRepository(_db);
    }

    public async Task InitializeAsync() =>
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");

    public Task DisposeAsync() => Task.CompletedTask;

    // GetAllByUserId
    [Fact]
    public async Task GetAllByUserId_ReturnsActiveAccounts_WhenUserHasAccounts()
    {
        var result = await _repo.GetAllByUserId(2);
        Assert.Equal(3, result.Count); // jsmith has Chase, BankOfAmerica, Chime
    }

    [Fact]
    public async Task GetAllByUserId_ExcludesInactiveAccounts()
    {
        await _repo.Deactivate(1); // deactivate jdoe's Chime account
        var result = await _repo.GetAllByUserId(1);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsEmpty_WhenNoAccounts()
    {
        var result = await _repo.GetAllByUserId(999);
        Assert.Empty(result);
    }

    // GetByUserAndBank
    [Fact]
    public async Task GetByUserAndBank_ReturnsAccount_WhenExists()
    {
        var result = await _repo.GetByUserAndBank(1, BankName.Chime);
        Assert.NotNull(result);
        Assert.Equal(5000.0000m, result.Balance);
    }

    [Fact]
    public async Task GetByUserAndBank_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByUserAndBank(1, BankName.Chase);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserAndBank_ReturnsInactiveAccount_WhenDeactivated()
    {
        await _repo.Deactivate(1);
        var result = await _repo.GetByUserAndBank(1, BankName.Chime);
        Assert.NotNull(result);          // still findable
        Assert.False(result.IsActive);   // but inactive
    }

    // Create
    [Fact]
    public async Task Create_ReturnsAccount_WithId()
    {
        var account = new BankAccount
        {
            AppUserId       = 1,
            BankName        = BankName.Chase,
            Nickname        = "My Chase",
            Balance         = 1000m,
            IsActive        = true,
            LastActivatedAt = DateTime.UtcNow,
            CreatedAt       = DateTime.UtcNow
        };

        var result = await _repo.Create(account);

        Assert.True(result.BankAccountId > 0);
        Assert.Equal(BankName.Chase, result.BankName);
    }

    // Update
    [Fact]
    public async Task Update_ReturnsUpdatedAccount_WhenExists()
    {
        var account = await _repo.GetById(1);
        account!.Balance = 9999m;

        var result = await _repo.Update(account);

        Assert.NotNull(result);
        Assert.Equal(9999m, result.Balance);
    }

    [Fact]
    public async Task Update_ReturnsNull_WhenNotFound()
    {
        var ghost = new BankAccount { BankAccountId = 999, Balance = 1m,
                                      LastActivatedAt = DateTime.UtcNow };
        var result = await _repo.Update(ghost);
        Assert.Null(result);
    }

    // Deactivate
    [Fact]
    public async Task Deactivate_ReturnsTrue_WhenExists()
    {
        var result = await _repo.Deactivate(1);
        Assert.True(result);

        var account = await _repo.GetById(1);
        Assert.False(account!.IsActive);
    }

    [Fact]
    public async Task Deactivate_ReturnsFalse_WhenNotFound()
    {
        var result = await _repo.Deactivate(999);
        Assert.False(result);
    }
}