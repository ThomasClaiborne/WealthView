using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.DTOs.Requests;
using Server.DTOs.Responses;
using Server.Models;

namespace Server.Domain;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepo,
        IPortfolioRepository portfolioRepo,
        AppDbContext db,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _portfolioRepo = portfolioRepo;
        _db = db;
        _config = config;
    }

    public async Task<Result<AuthResponse>> Register(RegisterRequest request)
    {
        var result = new Result<AuthResponse>();

        // Validate uniqueness before writing anything
        if (await _userRepo.GetByUsername(request.Username) != null)
        {
            result.AddMessage("Username is already taken.", ResultType.Invalid);
            return result;
        }

        if (await _userRepo.GetByEmail(request.Email) != null)
        {
            result.AddMessage("Email is already registered.", ResultType.Invalid);
            return result;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var user = new AppUser
            {
                Username = request.Username,
                Email    = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName  = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.Create(user);

            await _portfolioRepo.Create(new Portfolio
            {
                AppUserId    = user.AppUserId,
                CashBalance  = 0,
                CreatedAt    = DateTime.UtcNow
            });

            await transaction.CommitAsync();

            var authResponse = ToAuthResponse(user, GenerateToken(user));
            result.Payload = authResponse;
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            result.AddMessage("Registration failed. Please try again.", ResultType.BadRequest);
            return result;
        }
    }

    public async Task<Result<AuthResponse>> Login(LoginRequest request)
    {
        var result = new Result<AuthResponse>();

        var user = request.Credential.Contains('@')
            ? await _userRepo.GetByEmail(request.Credential)
            : await _userRepo.GetByUsername(request.Credential);

        if (user == null) {
            result.AddMessage("Invalid credentials.", ResultType.NotFound);
            return result;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) {
            result.AddMessage("Invalid credentials.", ResultType.Invalid);
            return result;
        }

        var authResponse = ToAuthResponse(user, GenerateToken(user));
        result.Payload = authResponse;
        return result;
    }

    // ── Private helpers ──────────────────────────────────────────────

    private string GenerateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: [
                new Claim(ClaimTypes.NameIdentifier, user.AppUserId.ToString()),
                new Claim(ClaimTypes.Name,  user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            ],
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthResponse ToAuthResponse(AppUser user, string token) => new()
    {
        Token     = token,
        AppUserId = user.AppUserId,
        Username  = user.Username,
        Email     = user.Email,
        FirstName = user.FirstName,
        LastName  = user.LastName
    };
}