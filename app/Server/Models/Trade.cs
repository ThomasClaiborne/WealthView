namespace Server.Models;

public class Trade
{
    public int       TradeId       { get; set; }
    public int       PortfolioId   { get; set; }
    public string    Ticker        { get; set; } = null!;
    public TradeType TradeType     { get; set; }
    public decimal   Quantity      { get; set; }
    public decimal   PricePerShare { get; set; }
    public decimal   TotalValue    { get; set; }
    public DateTime  ExecutedAt    { get; set; }

    // Navigation
    public Portfolio Portfolio { get; set; } = null!;
}