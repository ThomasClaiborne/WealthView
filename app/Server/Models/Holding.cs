namespace Server.Models;

public class Holding
{
    public int     HoldingId   { get; set; }
    public int     PortfolioId { get; set; }
    public string  Ticker      { get; set; } = null!;
    public decimal Quantity    { get; set; }
    public decimal AvgCost     { get; set; }
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }

    // Navigation
    public Portfolio Portfolio { get; set; } = null!;
    public Security  Security  { get; set; } = null!;
}