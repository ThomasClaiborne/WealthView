namespace Server.Models;

public class Security
{
    public string Ticker { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public AssetClass AssetClass { get; set; }
    public decimal? LastPrice { get; set; }
    public DateTime? PriceFetchedAt { get; set; }

    // Navigation properties
    public List<Holding> Holdings { get; set; } = new();
}