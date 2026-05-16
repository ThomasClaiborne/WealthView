namespace Server.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class FundTransferRequest
{
    [Required]
    public int BankAccountId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}