namespace Server.Models;

public class FundTransfer
{
    public int               FundTransferId  { get; set; }
    public int               PortfolioId     { get; set; }
    public int               BankAccountId   { get; set; }
    public TransferDirection Direction       { get; set; }
    public decimal           Amount          { get; set; }
    public TransferStatus    Status          { get; set; }
    public DateTime          CreatedAt       { get; set; }
    public DateTime?         ResolvedAt      { get; set; }

    // Navigation
    public Portfolio    Portfolio    { get; set; } = null!;
    public BankAccount  BankAccount  { get; set; } = null!;
}