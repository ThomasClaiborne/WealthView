namespace Server.DTOs.Responses;

public class PortfolioResponse
{
    public int PortfolioId { get; set; }
    public decimal CashBalance { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalUnrealizedGl { get; set; }
    public int HoldingCount { get; set; }
}