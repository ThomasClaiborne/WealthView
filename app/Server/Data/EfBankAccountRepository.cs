namespace Server.Data;

using Microsoft.EntityFrameworkCore;
using Server.Models;

public class EfBankAccountRepository : IBankAccountRepository
{
    private readonly AppDbContext _db;

    public EfBankAccountRepository(AppDbContext db) => _db = db;

    public async Task<List<BankAccount>> GetAllByUserId(int userId)
    {
        return await _db.BankAccounts
            .AsNoTracking()
            .Where(b => b.AppUserId == userId && b.IsActive)
            .OrderBy(b => b.BankName)
            .ToListAsync();
    }

    public async Task<BankAccount?> GetById(int bankAccountId)
    {
        return await _db.BankAccounts
            .FirstOrDefaultAsync(b => b.BankAccountId == bankAccountId);
    }

    public async Task<BankAccount?> GetByUserAndBank(int userId, BankName bankName)
    {
        return await _db.BankAccounts
            .FirstOrDefaultAsync(b => b.AppUserId == userId && b.BankName == bankName);
    }

    public async Task<BankAccount> Create(BankAccount account)
    {
        _db.BankAccounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task<BankAccount?> Update(BankAccount account)
    {
        var existing = await _db.BankAccounts.FindAsync(account.BankAccountId);
        if (existing is null) return null;

        existing.Balance         = account.Balance;
        existing.IsActive        = account.IsActive;
        existing.Nickname        = account.Nickname;
        existing.LastActivatedAt = account.LastActivatedAt;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> Deactivate(int bankAccountId)
    {
        var account = await _db.BankAccounts.FindAsync(bankAccountId);
        if (account is null) return false;

        account.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}