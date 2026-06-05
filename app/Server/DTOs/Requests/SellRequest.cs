namespace Server.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class SellRequest
{
    [Required]
    public string Ticker { get; set; } = null!;

    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }
}