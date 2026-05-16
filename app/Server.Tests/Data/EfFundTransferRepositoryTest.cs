namespace Server.Tests.Data;

using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

[Collection("DatabaseTests")]
public class EfFundTransferRepositoryTest : IAsyncLifetime
{
    private readonly AppDbContext             _db;
    private readonly EfFundTransferRepository _repo;

    public EfFundTransferRepositoryTest()
    {
        _db   = TestDbContextFactory.Create();
        _repo = new EfFundTransferRepository(_db);
    }

    public async Task InitializeAsync() =>
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");

    public Task DisposeAsync() => Task.CompletedTask;

    // GetPendingByPortfolioId
    [Fact]
    public async Task GetPendingByPortfolioId_ReturnsPendingOnly()
    {
        // portfolio 2 has one Pending transfer (id=3) and one Approved (id=2)
        var result = await _repo.GetPendingByPortfolioId(2);
        Assert.Single(result);
        Assert.Equal(TransferStatus.Pending, result[0].Status);
    }

    [Fact]
    public async Task GetPendingByPortfolioId_ReturnsEmpty_WhenNoPending()
    {
        // portfolio 1 has only Approved transfers
        var result = await _repo.GetPendingByPortfolioId(1);
        Assert.Empty(result);
    }

    // GetHistoryByPortfolioId
    [Fact]
    public async Task GetHistoryByPortfolioId_ReturnsResolvedOnly()
    {
        // portfolio 2 has one Approved (id=2) and one Pending (id=3)
        // history should return only the Approved one
        var result = await _repo.GetHistoryByPortfolioId(2);
        Assert.Single(result);
        Assert.Equal(TransferStatus.Approved, result[0].Status);
    }

    [Fact]
    public async Task GetHistoryByPortfolioId_ReturnsEmpty_WhenNoHistory()
    {
        var result = await _repo.GetHistoryByPortfolioId(999);
        Assert.Empty(result);
    }

    // GetById
    [Fact]
    public async Task GetById_ReturnsTransfer_WhenExists()
    {
        var result = await _repo.GetById(1);
        Assert.NotNull(result);
        Assert.Equal(TransferDirection.Deposit, result.Direction);
        Assert.Equal(2500.0000m, result.Amount);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetById(999);
        Assert.Null(result);
    }

    // Create
    [Fact]
    public async Task Create_ReturnsTransfer_WithId()
    {
        var transfer = new FundTransfer
        {
            PortfolioId   = 1,
            BankAccountId = 1,
            Direction     = TransferDirection.Deposit,
            Amount        = 500m,
            Status        = TransferStatus.Pending,
            CreatedAt     = DateTime.UtcNow
        };

        var result = await _repo.Create(transfer);

        Assert.True(result.FundTransferId > 0);
        Assert.Equal(TransferStatus.Pending, result.Status);
    }

    // Update
    [Fact]
    public async Task Update_ReturnsUpdatedTransfer_WhenExists()
    {
        // transfer id=3 is Pending — approve it
        var transfer = await _repo.GetById(3);
        transfer!.Status     = TransferStatus.Approved;
        transfer.ResolvedAt  = DateTime.UtcNow;

        var result = await _repo.Update(transfer);

        Assert.NotNull(result);
        Assert.Equal(TransferStatus.Approved, result.Status);
        Assert.NotNull(result.ResolvedAt);
    }

    [Fact]
    public async Task Update_ReturnsNull_WhenNotFound()
    {
        var ghost = new FundTransfer
        {
            FundTransferId = 999,
            Status         = TransferStatus.Rejected,
            ResolvedAt     = DateTime.UtcNow
        };
        var result = await _repo.Update(ghost);
        Assert.Null(result);
    }
}