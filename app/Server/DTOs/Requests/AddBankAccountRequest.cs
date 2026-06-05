namespace Server.DTOs.Requests;

using System.ComponentModel.DataAnnotations;
using Server.Models;

public class AddBankAccountRequest
{
    [Required]
    public BankName BankName { get; set; }

    public string? Nickname { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StartingBalance { get; set; }
}