namespace Server.DTOs.Responses;

using Server.Models;

public class HoldingResponse
{
    public int        HoldingId       { get; set; }
    public int        PortfolioId     { get; set; }
    public string     Ticker          { get; set; } = null!;
    public string     CompanyName     { get; set; } = null!;
    public AssetClass AssetClass      { get; set; }
    public decimal    Quantity        { get; set; }
    public decimal    AvgCost         { get; set; }
    public decimal    CurrentPrice    { get; set; }
    public decimal    MarketValue     { get; set; }
    public decimal    UnrealizedGl    { get; set; }
    public decimal    UnrealizedGlPct { get; set; }
    public decimal    PortfolioWeight { get; set; }
}