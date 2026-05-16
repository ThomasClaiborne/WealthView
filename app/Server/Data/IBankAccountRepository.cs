namespace Server.Data;

using Server.Models;

public interface IBankAccountRepository
{
    Task<List<BankAccount>> GetAllByUserId(int userId);
    Task<BankAccount?>      GetById(int bankAccountId);
    Task<BankAccount?>      GetByUserAndBank(int userId, BankName bankName);
    Task<BankAccount>       Create(BankAccount account);
    Task<BankAccount?>      Update(BankAccount account);
    Task<bool>              Deactivate(int bankAccountId);
}