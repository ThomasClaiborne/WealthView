namespace Server.Tests.Domain;

using Moq;
using Server.Data;
using Server.Domain;
using Server.DTOs.Requests;
using Server.Models;

public class BankAccountServiceTest
{
    private readonly Mock<IBankAccountRepository> _repo = new();
    private readonly BankAccountService           _service;

    public BankAccountServiceTest()
    {
        _service = new BankAccountService(_repo.Object);
    }

    [Fact]
    public async Task Add_ReturnsConflict_WhenActiveBankAccountExists()
    {
        _repo.Setup(r => r.GetByUserAndBank(1, BankName.Chase))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, IsActive = true });

        var result = await _service.Add(1, new AddBankAccountRequest
            { BankName = BankName.Chase, StartingBalance = 500m });

        Assert.Equal(ResultType.Conflict, result.Type);
    }

    [Fact]
    public async Task Add_ReactivatesAccount_WhenInactiveAccountExists()
    {
        var inactive = new BankAccount
            { BankAccountId = 1, AppUserId = 1, BankName = BankName.Chase,
              IsActive = false, Balance = 0m, LastActivatedAt = DateTime.UtcNow };

        _repo.Setup(r => r.GetByUserAndBank(1, BankName.Chase)).ReturnsAsync(inactive);
        _repo.Setup(r => r.Update(It.IsAny<BankAccount>()))
            .ReturnsAsync((BankAccount b) => b);

        var result = await _service.Add(1, new AddBankAccountRequest
            { BankName = BankName.Chase, StartingBalance = 1000m });

        Assert.Equal(ResultType.Success, result.Type);
        _repo.Verify(r => r.Update(It.Is<BankAccount>(
            b => b.IsActive && b.Balance == 1000m)), Times.Once);
        _repo.Verify(r => r.Create(It.IsAny<BankAccount>()), Times.Never);
    }

    [Fact]
    public async Task Add_CreatesNew_WhenNoPriorAccount()
    {
        _repo.Setup(r => r.GetByUserAndBank(1, BankName.Chime))
            .ReturnsAsync((BankAccount?)null);
        _repo.Setup(r => r.Create(It.IsAny<BankAccount>()))
            .ReturnsAsync((BankAccount b) => { b.BankAccountId = 1; return b; });

        var result = await _service.Add(1, new AddBankAccountRequest
            { BankName = BankName.Chime, StartingBalance = 500m });

        Assert.Equal(ResultType.Success, result.Type);
        _repo.Verify(r => r.Create(It.IsAny<BankAccount>()), Times.Once);
        _repo.Verify(r => r.Update(It.IsAny<BankAccount>()), Times.Never);
    }

    [Fact]
    public async Task Deposit_ReturnsNotFound_WhenAccountNotFound()
    {
        _repo.Setup(r => r.GetById(99)).ReturnsAsync((BankAccount?)null);

        var result = await _service.Deposit(99, 1, new AdjustBalanceRequest { Amount = 100m });

        Assert.Equal(ResultType.NotFound, result.Type);
    }

    [Fact]
    public async Task Deposit_ReturnsNotFound_WhenAccountBelongsToOtherUser()
    {
        _repo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, AppUserId = 2 });

        var result = await _service.Deposit(1, 1, new AdjustBalanceRequest { Amount = 100m });

        Assert.Equal(ResultType.NotFound, result.Type);
    }

    [Fact]
    public async Task Deposit_UpdatesBalance_WhenValid()
    {
        _repo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, AppUserId = 1, Balance = 500m,
                                            BankName = BankName.Chime, LastActivatedAt = DateTime.UtcNow });
        _repo.Setup(r => r.Update(It.IsAny<BankAccount>()))
            .ReturnsAsync((BankAccount b) => b);

        var result = await _service.Deposit(1, 1, new AdjustBalanceRequest { Amount = 200m });

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(700m, result.Payload!.Balance);
    }

    [Fact]
    public async Task Withdraw_ReturnsInvalid_WhenInsufficientFunds()
    {
        _repo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, AppUserId = 1, Balance = 100m });

        var result = await _service.Withdraw(1, 1, new AdjustBalanceRequest { Amount = 500m });

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task Withdraw_UpdatesBalance_WhenValid()
    {
        _repo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, AppUserId = 1, Balance = 1000m,
                                            BankName = BankName.Chase, LastActivatedAt = DateTime.UtcNow });
        _repo.Setup(r => r.Update(It.IsAny<BankAccount>()))
            .ReturnsAsync((BankAccount b) => b);

        var result = await _service.Withdraw(1, 1, new AdjustBalanceRequest { Amount = 300m });

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(700m, result.Payload!.Balance);
    }

    [Fact]
    public async Task Deactivate_ReturnsForbidden_WhenAccountBelongsToOtherUser()
    {
        _repo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, AppUserId = 2 });

        var result = await _service.Deactivate(1, 1);

        Assert.Equal(ResultType.Forbidden, result.Type);
    }

    [Fact]
    public async Task Deactivate_ReturnsSuccess_WhenOwnerDeactivates()
    {
        _repo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, AppUserId = 1 });
        _repo.Setup(r => r.Deactivate(1)).ReturnsAsync(true);

        var result = await _service.Deactivate(1, 1);

        Assert.Equal(ResultType.Success, result.Type);
        Assert.True(result.Payload);
    }
}