namespace Server.Domain;

using Server.Data;
using Server.DTOs.Requests;
using Server.DTOs.Responses;
using Server.Models;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepo;

    public BankAccountService(IBankAccountRepository bankAccountRepo)
    {
        _bankAccountRepo = bankAccountRepo;
    }

    public async Task<Result<List<BankAccountResponse>>> GetAllByUserId(int userId)
    {
        var result   = new Result<List<BankAccountResponse>>();
        var accounts = await _bankAccountRepo.GetAllByUserId(userId);
        result.Payload = accounts.Select(ToResponse).ToList();
        return result;
    }

    public async Task<Result<BankAccountResponse>> Add(int userId, AddBankAccountRequest request)
    {
        var result   = new Result<BankAccountResponse>();
        var existing = await _bankAccountRepo.GetByUserAndBank(userId, request.BankName);

        if (existing is not null && existing.IsActive)
        {
            result.AddMessage($"You already have an active {request.BankName} account.", ResultType.Conflict);
            return result;
        }

        if (existing is not null && !existing.IsActive)
        {
            // reactivate
            existing.IsActive        = true;
            existing.Balance         = request.StartingBalance;
            existing.Nickname        = request.Nickname;
            existing.LastActivatedAt = DateTime.UtcNow;

            var reactivated = await _bankAccountRepo.Update(existing);
            result.Payload = ToResponse(reactivated!);
            return result;
        }

        // brand new account
        var now     = DateTime.UtcNow;
        var account = new BankAccount
        {
            AppUserId       = userId,
            BankName        = request.BankName,
            Nickname        = request.Nickname,
            Balance         = request.StartingBalance,
            IsActive        = true,
            LastActivatedAt = now,
            CreatedAt       = now
        };

        var created = await _bankAccountRepo.Create(account);
        result.Payload = ToResponse(created);
        return result;
    }

    public async Task<Result<BankAccountResponse>> Deposit(int bankAccountId, int userId, AdjustBalanceRequest request)
    {
        var result  = new Result<BankAccountResponse>();
        var account = await _bankAccountRepo.GetById(bankAccountId);

        if (account is null || account.AppUserId != userId)
        {
            result.AddMessage("Bank account not found.", ResultType.NotFound);
            return result;
        }

        account.Balance += request.Amount;
        var updated = await _bankAccountRepo.Update(account);
        result.Payload  = ToResponse(updated!);
        return result;
    }

    public async Task<Result<BankAccountResponse>> Withdraw(int bankAccountId, int userId, AdjustBalanceRequest request)
    {
        var result  = new Result<BankAccountResponse>();
        var account = await _bankAccountRepo.GetById(bankAccountId);

        if (account is null || account.AppUserId != userId)
        {
            result.AddMessage("Bank account not found.", ResultType.NotFound);
            return result;
        }

        if (account.Balance < request.Amount)
        {
            result.AddMessage("Insufficient bank account balance.", ResultType.Invalid);
            return result;
        }

        account.Balance -= request.Amount;
        var updated = await _bankAccountRepo.Update(account);
        result.Payload  = ToResponse(updated!);
        return result;
    }

    public async Task<Result<bool>> Deactivate(int bankAccountId, int userId)
    {
        var result  = new Result<bool>();
        var account = await _bankAccountRepo.GetById(bankAccountId);

        if (account is null)
        {
            result.AddMessage("Bank account not found.", ResultType.NotFound);
            return result;
        }

        if (account.AppUserId != userId)
        {
            result.AddMessage("Forbidden.", ResultType.Forbidden);
            return result;
        }

        await _bankAccountRepo.Deactivate(bankAccountId);
        result.Payload = true;
        return result;
    }

    private static BankAccountResponse ToResponse(BankAccount b) => new()
    {
        BankAccountId   = b.BankAccountId,
        BankName        = b.BankName.ToString(),
        Nickname        = b.Nickname,
        Balance         = b.Balance,
        IsActive        = b.IsActive,
        LastActivatedAt = b.LastActivatedAt,
        CreatedAt       = b.CreatedAt
    };
}