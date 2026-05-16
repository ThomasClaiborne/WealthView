namespace Server.Data;

using Server.Models;

public interface IFundTransferRepository
{
    Task<List<FundTransfer>> GetPendingByPortfolioId(int portfolioId);
    Task<List<FundTransfer>> GetHistoryByPortfolioId(int portfolioId);
    Task<FundTransfer?>      GetById(int fundTransferId);
    Task<FundTransfer>       Create(FundTransfer transfer);
    Task<FundTransfer?>      Update(FundTransfer transfer);
}