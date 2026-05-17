namespace Server.Models;

public class PortfolioSnapshot
{
    public int      SnapshotId    { get; set; }
    public int      PortfolioId   { get; set; }
    public DateOnly SnapshotDate  { get; set; }
    public decimal  TotalValue    { get; set; }

    // Navigation
    public Portfolio Portfolio { get; set; } = null!;
}