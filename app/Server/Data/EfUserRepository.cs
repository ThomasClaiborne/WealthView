
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public EfUserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AppUser?> GetByUsername(string username)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<AppUser?> GetByEmail(string email)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetById(int id)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.AppUserId == id);
    }

    public async Task<AppUser> Create(AppUser user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}