using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Server.Data;
using Server.Domain;
using Server.DTOs.Requests;

namespace Server.Tests.Domain;

[Collection("DatabaseTests")]
public class AuthServiceTest
{
    private AuthService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]         = "test_secret_key_minimum_32_characters_long!",
                ["Jwt:Issuer"]         = "WealthView",
                ["Jwt:Audience"]       = "WealthViewUsers",
                ["Jwt:ExpiryMinutes"]  = "60"
            })
            .Build();

        return new AuthService(
            new EfUserRepository(db),
            new EfPortfolioRepository(db),
            db,
            config);
    }

    // Helper — avoids repeating the same RegisterRequest in every test
    private static RegisterRequest ValidRegisterRequest(
        string username = "johndoe",
        string email    = "john@test.com") => new()
    {
        FirstName = "John",
        LastName  = "Doe",
        Username  = username,
        Email     = email,
        Password  = "Password1!"
    };

    // ── Register ─────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ReturnsSuccess_WhenRequestIsValid()
    {
        var service = CreateService();

        var result = await service.Register(ValidRegisterRequest());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Payload);
        Assert.False(string.IsNullOrEmpty(result.Payload.Token));
        Assert.Equal("johndoe", result.Payload.Username);
        Assert.Equal("john@test.com", result.Payload.Email);
    }

    [Fact]
    public async Task Register_ReturnsInvalid_WhenUsernameIsTaken()
    {
        var service = CreateService();
        await service.Register(ValidRegisterRequest());

        // Same username, different email
        var result = await service.Register(
            ValidRegisterRequest(username: "johndoe", email: "other@test.com"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.Invalid, result.Type);
        Assert.Contains("Username", result.Messages[0]);
    }

    [Fact]
    public async Task Register_ReturnsInvalid_WhenEmailIsTaken()
    {
        var service = CreateService();
        await service.Register(ValidRegisterRequest());

        // Different username, same email
        var result = await service.Register(
            ValidRegisterRequest(username: "janedoe", email: "john@test.com"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.Invalid, result.Type);
        Assert.Contains("Email", result.Messages[0]);
    }

    // ── Login ────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ReturnsSuccess_WhenCredentialIsUsername()
    {
        var service = CreateService();
        await service.Register(ValidRegisterRequest());

        var result = await service.Login(new LoginRequest
        {
            Credential = "johndoe",
            Password   = "Password1!"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Payload);
        Assert.False(string.IsNullOrEmpty(result.Payload.Token));
        Assert.Equal("johndoe", result.Payload.Username);
    }

    [Fact]
    public async Task Login_ReturnsSuccess_WhenCredentialIsEmail()
    {
        var service = CreateService();
        await service.Register(ValidRegisterRequest());

        var result = await service.Login(new LoginRequest
        {
            Credential = "john@test.com",
            Password   = "Password1!"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Payload);
        Assert.False(string.IsNullOrEmpty(result.Payload.Token));
    }

    [Fact]
    public async Task Login_ReturnsNotFound_WhenCredentialDoesNotExist()
    {
        var service = CreateService();

        var result = await service.Login(new LoginRequest
        {
            Credential = "nobody",
            Password   = "Password1!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.NotFound, result.Type);
    }

    [Fact]
    public async Task Login_ReturnsInvalid_WhenPasswordIsWrong()
    {
        var service = CreateService();
        await service.Register(ValidRegisterRequest());

        var result = await service.Login(new LoginRequest
        {
            Credential = "johndoe",
            Password   = "WrongPassword!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultType.Invalid, result.Type);
    }
}