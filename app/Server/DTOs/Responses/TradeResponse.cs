namespace Server.DTOs.Responses;

using Server.Models;

public class TradeResponse
{
    public int       TradeId        { get; set; }
    public string    Ticker         { get; set; } = null!;
    public TradeType TradeType      { get; set; }
    public decimal   Quantity       { get; set; }
    public decimal   PricePerShare  { get; set; }
    public decimal   TotalValue     { get; set; }
    public DateTime  ExecutedAt     { get; set; }
    public decimal   NewCashBalance { get; set; }
}