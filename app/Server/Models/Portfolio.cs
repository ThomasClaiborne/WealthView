namespace Server.Models;

public class Portfolio
{
    public int PortfolioId { get; set; }
    public int AppUserId { get; set; }
    public decimal CashBalance { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public AppUser AppUser { get; set; } = null!;
}