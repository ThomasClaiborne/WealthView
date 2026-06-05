namespace Server.Domain;

using Server.Data;
using Server.DTOs.Requests;
using Server.DTOs.Responses;
using Server.Models;

public class FundTransferService : IFundTransferService
{
    private readonly IFundTransferRepository  _transferRepo;
    private readonly IBankAccountRepository   _bankAccountRepo;
    private readonly IPortfolioRepository     _portfolioRepo;

    public FundTransferService(
        IFundTransferRepository  transferRepo,
        IBankAccountRepository   bankAccountRepo,
        IPortfolioRepository     portfolioRepo)
    {
        _transferRepo    = transferRepo;
        _bankAccountRepo = bankAccountRepo;
        _portfolioRepo   = portfolioRepo;
    }

    public async Task<Result<List<FundTransferResponse>>> GetPending(int portfolioId)
    {
        var result    = new Result<List<FundTransferResponse>>();
        var transfers = await _transferRepo.GetPendingByPortfolioId(portfolioId);
        result.Payload = transfers.Select(ToResponse).ToList();
        return result;
    }

    public async Task<Result<List<FundTransferResponse>>> GetHistory(int portfolioId)
    {
        var result    = new Result<List<FundTransferResponse>>();
        var transfers = await _transferRepo.GetHistoryByPortfolioId(portfolioId);
        result.Payload = transfers.Select(ToResponse).ToList();
        return result;
    }

    public async Task<Result<FundTransferResponse>> RequestDeposit(int portfolioId, FundTransferRequest request)
    {
        var result  = new Result<FundTransferResponse>();
        var account = await _bankAccountRepo.GetById(request.BankAccountId);

        if (account is null)
        {
            result.AddMessage("Bank account not found.", ResultType.NotFound);
            return result;
        }

        if (account.Balance < request.Amount)
        {
            result.AddMessage("Insufficient bank account balance.", ResultType.Invalid);
            return result;
        }

        var transfer = await _transferRepo.Create(new FundTransfer
        {
            PortfolioId   = portfolioId,
            BankAccountId = request.BankAccountId,
            Direction     = TransferDirection.Deposit,
            Amount        = request.Amount,
            Status        = TransferStatus.Pending,
            CreatedAt     = DateTime.UtcNow
        });

        // re-fetch to get BankAccount navigation populated
        var fetched   = await _transferRepo.GetById(transfer.FundTransferId);
        result.Payload = ToResponse(fetched!);
        return result;
    }

    public async Task<Result<FundTransferResponse>> RequestWithdrawal(int portfolioId, FundTransferRequest request)
    {
        var result    = new Result<FundTransferResponse>();
        var portfolio = await _portfolioRepo.GetById(portfolioId);

        if (portfolio is null)
        {
            result.AddMessage("Portfolio not found.", ResultType.NotFound);
            return result;
        }

        if (portfolio.CashBalance < request.Amount)
        {
            result.AddMessage("Insufficient portfolio cash balance.", ResultType.Invalid);
            return result;
        }

        var account = await _bankAccountRepo.GetById(request.BankAccountId);
        if (account is null)
        {
            result.AddMessage("Bank account not found.", ResultType.NotFound);
            return result;
        }

        var transfer = await _transferRepo.Create(new FundTransfer
        {
            PortfolioId   = portfolioId,
            BankAccountId = request.BankAccountId,
            Direction     = TransferDirection.Withdrawal,
            Amount        = request.Amount,
            Status        = TransferStatus.Pending,
            CreatedAt     = DateTime.UtcNow
        });

        var fetched   = await _transferRepo.GetById(transfer.FundTransferId);
        result.Payload = ToResponse(fetched!);
        return result;
    }

    public async Task<Result<FundTransferResponse>> Approve(int fundTransferId, int portfolioId)
    {
        var result   = new Result<FundTransferResponse>();
        var transfer = await _transferRepo.GetById(fundTransferId);

        if (transfer is null || transfer.PortfolioId != portfolioId)
        {
            result.AddMessage("Transfer not found.", ResultType.NotFound);
            return result;
        }

        if (transfer.Status != TransferStatus.Pending)
        {
            result.AddMessage("Only pending transfers can be approved.", ResultType.Invalid);
            return result;
        }

        var portfolio = await _portfolioRepo.GetById(portfolioId);
        var account   = await _bankAccountRepo.GetById(transfer.BankAccountId);

        if (transfer.Direction == TransferDirection.Deposit)
        {
            // bank → portfolio
            account!.Balance        -= transfer.Amount;
            portfolio!.CashBalance  += transfer.Amount;
        }
        else
        {
            // portfolio → bank
            portfolio!.CashBalance  -= transfer.Amount;
            account!.Balance        += transfer.Amount;
        }

        await _bankAccountRepo.Update(account);
        await _portfolioRepo.UpdateCashBalance(portfolioId, portfolio.CashBalance);

        transfer.Status     = TransferStatus.Approved;
        transfer.ResolvedAt = DateTime.UtcNow;
        var updated = await _transferRepo.Update(transfer);

        result.Payload = ToResponse(updated!);
        return result;
    }

    public async Task<Result<FundTransferResponse>> Reject(int fundTransferId, int portfolioId)
    {
        var result   = new Result<FundTransferResponse>();
        var transfer = await _transferRepo.GetById(fundTransferId);

        if (transfer is null || transfer.PortfolioId != portfolioId)
        {
            result.AddMessage("Transfer not found.", ResultType.NotFound);
            return result;
        }

        if (transfer.Status != TransferStatus.Pending)
        {
            result.AddMessage("Only pending transfers can be rejected.", ResultType.Invalid);
            return result;
        }

        transfer.Status     = TransferStatus.Rejected;
        transfer.ResolvedAt = DateTime.UtcNow;
        var updated = await _transferRepo.Update(transfer);

        result.Payload = ToResponse(updated!);
        return result;
    }

    private static FundTransferResponse ToResponse(FundTransfer f) => new()
    {
        FundTransferId = f.FundTransferId,
        PortfolioId    = f.PortfolioId,
        BankAccountId  = f.BankAccountId,
        BankName       = f.BankAccount.BankName.ToString(),
        BankNickname   = f.BankAccount.Nickname,
        Direction      = f.Direction.ToString(),
        Amount         = f.Amount,
        Status         = f.Status.ToString(),
        CreatedAt      = f.CreatedAt,
        ResolvedAt     = f.ResolvedAt
    };
}