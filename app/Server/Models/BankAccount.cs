namespace Server.Models;

public class BankAccount
{
    public int      BankAccountId   { get; set; }
    public int      AppUserId       { get; set; }
    public BankName BankName        { get; set; }
    public string?  Nickname        { get; set; }
    public decimal  Balance         { get; set; }
    public bool     IsActive        { get; set; }
    public DateTime LastActivatedAt { get; set; }
    public DateTime CreatedAt       { get; set; }

    // Navigation
    public AppUser AppUser { get; set; } = null!;
}