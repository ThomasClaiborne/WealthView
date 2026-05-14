using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Tests;

[Collection("DatabaseTests")]   
public class EfUserRepositoryTest : IAsyncLifetime
{
    private AppDbContext _db = null!;
    private EfUserRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.Create();
        _repo = new EfUserRepository(_db);
        await _db.Database.ExecuteSqlRawAsync("CALL set_known_good_state()");
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    // ── GetByUsername ────────────────────────────────────────────────

    [Fact]
    public async Task GetByUsername_ReturnsUser_WhenUsernameExists()
    {
        var result = await _repo.GetByUsername("jdoe");

        Assert.NotNull(result);
        Assert.Equal("jdoe", result.Username);
        Assert.Equal("john@wealthview.com", result.Email);
    }

    [Fact]
    public async Task GetByUsername_ReturnsNull_WhenUsernameNotFound()
    {
        var result = await _repo.GetByUsername("nobody");

        Assert.Null(result);
    }

    // ── GetByEmail ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmail_ReturnsUser_WhenEmailExists()
    {
        var result = await _repo.GetByEmail("john@wealthview.com");

        Assert.NotNull(result);
        Assert.Equal("jdoe", result.Username);
    }

    [Fact]
    public async Task GetByEmail_ReturnsNull_WhenEmailNotFound()
    {
        var result = await _repo.GetByEmail("nobody@test.com");

        Assert.Null(result);
    }

    // ── GetById ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsUser_WhenIdExists()
    {
        var result = await _repo.GetById(1);

        Assert.NotNull(result);
        Assert.Equal("jdoe", result.Username);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenIdNotFound()
    {
        var result = await _repo.GetById(999);

        Assert.Null(result);
    }

    // ── Create ───────────────────────────────────────────────────────

    [Fact]
    public async Task Create_InsertsUser_AndReturnsWithGeneratedId()
    {
        var newUser = new AppUser
        {
            Username = "newuser",
            Email = "new@wealthview.com",
            PasswordHash = "hashedpassword",
            FirstName = "New",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repo.Create(newUser);

        Assert.True(result.AppUserId > 0);

        var fromDb = await _repo.GetById(result.AppUserId);
        Assert.NotNull(fromDb);
        Assert.Equal("newuser", fromDb.Username);
        Assert.Equal("new@wealthview.com", fromDb.Email);
    }
}