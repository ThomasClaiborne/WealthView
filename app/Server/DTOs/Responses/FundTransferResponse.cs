namespace Server.DTOs.Responses;

public class FundTransferResponse
{
    public int       FundTransferId  { get; set; }
    public int       PortfolioId     { get; set; }
    public int       BankAccountId   { get; set; }
    public string    BankName        { get; set; } = null!;
    public string?   BankNickname    { get; set; }
    public string    Direction       { get; set; } = null!;
    public decimal   Amount          { get; set; }
    public string    Status          { get; set; } = null!;
    public DateTime  CreatedAt       { get; set; }
    public DateTime? ResolvedAt      { get; set; }
}