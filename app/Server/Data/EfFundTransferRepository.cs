namespace Server.Data;

using Microsoft.EntityFrameworkCore;
using Server.Models;

public class EfFundTransferRepository : IFundTransferRepository
{
    private readonly AppDbContext _db;

    public EfFundTransferRepository(AppDbContext db) => _db = db;

    public async Task<List<FundTransfer>> GetPendingByPortfolioId(int portfolioId)
    {
        return await _db.FundTransfers
            .AsNoTracking()
            .Include(f => f.BankAccount)
            .Where(f => f.PortfolioId == portfolioId && f.Status == TransferStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<FundTransfer>> GetHistoryByPortfolioId(int portfolioId)
    {
        return await _db.FundTransfers
            .AsNoTracking()
            .Include(f => f.BankAccount)
            .Where(f => f.PortfolioId == portfolioId && f.Status != TransferStatus.Pending)
            .OrderByDescending(f => f.ResolvedAt)
            .ToListAsync();
    }

    public async Task<FundTransfer?> GetById(int fundTransferId)
    {
        return await _db.FundTransfers
            .Include(f => f.BankAccount)
            .FirstOrDefaultAsync(f => f.FundTransferId == fundTransferId);
    }

    public async Task<FundTransfer> Create(FundTransfer transfer)
    {
        _db.FundTransfers.Add(transfer);
        await _db.SaveChangesAsync();
        return transfer;
    }

    public async Task<FundTransfer?> Update(FundTransfer transfer)
    {
        var existing = await _db.FundTransfers.FindAsync(transfer.FundTransferId);
        if (existing is null) return null;

        existing.Status     = transfer.Status;
        existing.ResolvedAt = transfer.ResolvedAt;

        await _db.SaveChangesAsync();
        return existing;
    }
}