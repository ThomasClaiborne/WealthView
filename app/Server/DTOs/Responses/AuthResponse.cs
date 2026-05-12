namespace Server.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public int AppUserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}