namespace Server.Domain;

using Server.DTOs.Requests;
using Server.DTOs.Responses;

public interface IFundTransferService
{
    Task<Result<FundTransferResponse>>       RequestDeposit(int portfolioId, FundTransferRequest request);
    Task<Result<FundTransferResponse>>       RequestWithdrawal(int portfolioId, FundTransferRequest request);
    Task<Result<FundTransferResponse>>       Approve(int fundTransferId, int portfolioId);
    Task<Result<FundTransferResponse>>       Reject(int fundTransferId, int portfolioId);
    Task<Result<List<FundTransferResponse>>> GetPending(int portfolioId);
    Task<Result<List<FundTransferResponse>>> GetHistory(int portfolioId);
}