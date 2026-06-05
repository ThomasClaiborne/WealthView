namespace Server.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class AdjustBalanceRequest
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}