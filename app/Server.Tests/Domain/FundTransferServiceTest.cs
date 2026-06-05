namespace Server.Tests.Domain;

using Moq;
using Server.Data;
using Server.Domain;
using Server.DTOs.Requests;
using Server.Models;

public class FundTransferServiceTest
{
    private readonly Mock<IFundTransferRepository>  _transferRepo    = new();
    private readonly Mock<IBankAccountRepository>   _bankAccountRepo = new();
    private readonly Mock<IPortfolioRepository>     _portfolioRepo   = new();
    private readonly FundTransferService            _service;

    public FundTransferServiceTest()
    {
        _service = new FundTransferService(
            _transferRepo.Object, _bankAccountRepo.Object, _portfolioRepo.Object);
    }

    // ── RequestDeposit ────────────────────────────────────────────

    [Fact]
    public async Task RequestDeposit_ReturnsNotFound_WhenBankAccountNotFound()
    {
        _bankAccountRepo.Setup(r => r.GetById(99)).ReturnsAsync((BankAccount?)null);

        var result = await _service.RequestDeposit(1,
            new FundTransferRequest { BankAccountId = 99, Amount = 500m });

        Assert.Equal(ResultType.NotFound, result.Type);
    }

    [Fact]
    public async Task RequestDeposit_ReturnsInvalid_WhenInsufficientBankBalance()
    {
        _bankAccountRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, Balance = 100m });

        var result = await _service.RequestDeposit(1,
            new FundTransferRequest { BankAccountId = 1, Amount = 500m });

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task RequestDeposit_CreatesPendingTransfer_WhenValid()
    {
        _bankAccountRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, Balance = 1000m,
                                            BankName = BankName.Chime });
        _transferRepo.Setup(r => r.Create(It.IsAny<FundTransfer>()))
            .ReturnsAsync((FundTransfer t) => { t.FundTransferId = 1; return t; });
        _transferRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new FundTransfer
            {
                FundTransferId = 1, PortfolioId = 1, BankAccountId = 1,
                Direction = TransferDirection.Deposit, Amount = 500m,
                Status = TransferStatus.Pending, CreatedAt = DateTime.UtcNow,
                BankAccount = new BankAccount { BankName = BankName.Chime }
            });

        var result = await _service.RequestDeposit(1,
            new FundTransferRequest { BankAccountId = 1, Amount = 500m });

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(TransferStatus.Pending.ToString(), result.Payload!.Status);
        _transferRepo.Verify(r => r.Create(It.IsAny<FundTransfer>()), Times.Once);
    }

    // ── RequestWithdrawal ─────────────────────────────────────────

    [Fact]
    public async Task RequestWithdrawal_ReturnsInvalid_WhenInsufficientPortfolioCash()
    {
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 50m });

        var result = await _service.RequestWithdrawal(1,
            new FundTransferRequest { BankAccountId = 1, Amount = 500m });

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task RequestWithdrawal_CreatesPendingTransfer_WhenValid()
    {
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 1000m });
        _bankAccountRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, BankName = BankName.Chase });
        _transferRepo.Setup(r => r.Create(It.IsAny<FundTransfer>()))
            .ReturnsAsync((FundTransfer t) => { t.FundTransferId = 2; return t; });
        _transferRepo.Setup(r => r.GetById(2))
            .ReturnsAsync(new FundTransfer
            {
                FundTransferId = 2, PortfolioId = 1, BankAccountId = 1,
                Direction = TransferDirection.Withdrawal, Amount = 300m,
                Status = TransferStatus.Pending, CreatedAt = DateTime.UtcNow,
                BankAccount = new BankAccount { BankName = BankName.Chase }
            });

        var result = await _service.RequestWithdrawal(1,
            new FundTransferRequest { BankAccountId = 1, Amount = 300m });

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(TransferStatus.Pending.ToString(), result.Payload!.Status);
    }

    // ── Approve ───────────────────────────────────────────────────

    [Fact]
    public async Task Approve_ReturnsInvalid_WhenTransferNotPending()
    {
        _transferRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new FundTransfer
                { FundTransferId = 1, PortfolioId = 1, Status = TransferStatus.Approved,
                  BankAccount = new BankAccount { BankName = BankName.Chime } });

        var result = await _service.Approve(1, 1);

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task Approve_ExecutesDeposit_MovesMoneyFromBankToPortfolio()
    {
        // bank balance: 1000, deposit: 500 → bank: 500, portfolio: +500
        _transferRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new FundTransfer
            {
                FundTransferId = 1, PortfolioId = 1, BankAccountId = 1,
                Direction = TransferDirection.Deposit, Amount = 500m,
                Status = TransferStatus.Pending,
                BankAccount = new BankAccount { BankAccountId = 1, BankName = BankName.Chime }
            });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 200m });
        _bankAccountRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, Balance = 1000m,
                                            BankName = BankName.Chime });
        _bankAccountRepo.Setup(r => r.Update(It.IsAny<BankAccount>()))
            .ReturnsAsync((BankAccount b) => b);
        _portfolioRepo.Setup(r => r.UpdateCashBalance(1, It.IsAny<decimal>()))
            .ReturnsAsync(true);
        _transferRepo.Setup(r => r.Update(It.IsAny<FundTransfer>()))
            .ReturnsAsync((FundTransfer t) => t);

        var result = await _service.Approve(1, 1);

        Assert.Equal(ResultType.Success, result.Type);
        _bankAccountRepo.Verify(r => r.Update(
            It.Is<BankAccount>(b => b.Balance == 500m)), Times.Once);
        _portfolioRepo.Verify(r => r.UpdateCashBalance(1, 700m), Times.Once);
    }

    [Fact]
    public async Task Approve_ExecutesWithdrawal_MovesMoneyFromPortfolioToBank()
    {
        // portfolio: 1000, withdrawal: 300 → portfolio: 700, bank: +300
        _transferRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new FundTransfer
            {
                FundTransferId = 1, PortfolioId = 1, BankAccountId = 1,
                Direction = TransferDirection.Withdrawal, Amount = 300m,
                Status = TransferStatus.Pending,
                BankAccount = new BankAccount { BankAccountId = 1, BankName = BankName.Chase }
            });
        _portfolioRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new Portfolio { PortfolioId = 1, CashBalance = 1000m });
        _bankAccountRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new BankAccount { BankAccountId = 1, Balance = 500m,
                                            BankName = BankName.Chase });
        _bankAccountRepo.Setup(r => r.Update(It.IsAny<BankAccount>()))
            .ReturnsAsync((BankAccount b) => b);
        _portfolioRepo.Setup(r => r.UpdateCashBalance(1, It.IsAny<decimal>()))
            .ReturnsAsync(true);
        _transferRepo.Setup(r => r.Update(It.IsAny<FundTransfer>()))
            .ReturnsAsync((FundTransfer t) => t);

        var result = await _service.Approve(1, 1);

        Assert.Equal(ResultType.Success, result.Type);
        _portfolioRepo.Verify(r => r.UpdateCashBalance(1, 700m), Times.Once);
        _bankAccountRepo.Verify(r => r.Update(
            It.Is<BankAccount>(b => b.Balance == 800m)), Times.Once);
    }

    // ── Reject ────────────────────────────────────────────────────

    [Fact]
    public async Task Reject_ReturnsInvalid_WhenTransferNotPending()
    {
        _transferRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new FundTransfer
                { FundTransferId = 1, PortfolioId = 1, Status = TransferStatus.Rejected,
                  BankAccount = new BankAccount { BankName = BankName.Chime } });

        var result = await _service.Reject(1, 1);

        Assert.Equal(ResultType.Invalid, result.Type);
    }

    [Fact]
    public async Task Reject_SetsRejectedStatus_NoBalanceChanges()
    {
        _transferRepo.Setup(r => r.GetById(1))
            .ReturnsAsync(new FundTransfer
            {
                FundTransferId = 1, PortfolioId = 1, BankAccountId = 1,
                Direction = TransferDirection.Deposit, Amount = 500m,
                Status = TransferStatus.Pending,
                BankAccount = new BankAccount { BankName = BankName.Chime }
            });
        _transferRepo.Setup(r => r.Update(It.IsAny<FundTransfer>()))
            .ReturnsAsync((FundTransfer t) => t);

        var result = await _service.Reject(1, 1);

        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal(TransferStatus.Rejected.ToString(), result.Payload!.Status);
        _bankAccountRepo.Verify(r => r.Update(It.IsAny<BankAccount>()), Times.Never);
        _portfolioRepo.Verify(r => r.UpdateCashBalance(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }
}