namespace Server.Domain;

using Server.DTOs.Requests;
using Server.DTOs.Responses;

public interface IBankAccountService
{
    Task<Result<List<BankAccountResponse>>> GetAllByUserId(int userId);
    Task<Result<BankAccountResponse>>       Add(int userId, AddBankAccountRequest request);
    Task<Result<BankAccountResponse>>       Deposit(int bankAccountId, int userId, AdjustBalanceRequest request);
    Task<Result<BankAccountResponse>>       Withdraw(int bankAccountId, int userId, AdjustBalanceRequest request);
    Task<Result<bool>>                      Deactivate(int bankAccountId, int userId);
}