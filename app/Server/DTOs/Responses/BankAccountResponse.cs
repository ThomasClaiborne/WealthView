namespace Server.DTOs.Responses;

using Server.Models;

public class BankAccountResponse
{
    public int      BankAccountId   { get; set; }
    public string   BankName        { get; set; } = null!;
    public string?  Nickname        { get; set; }
    public decimal  Balance         { get; set; }
    public bool     IsActive        { get; set; }
    public DateTime LastActivatedAt { get; set; }
    public DateTime CreatedAt       { get; set; }
}