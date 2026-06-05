using System.ComponentModel.DataAnnotations;

namespace Server.DTOs.Requests;

public class LoginRequest
{
    [Required]
    public string Credential { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}