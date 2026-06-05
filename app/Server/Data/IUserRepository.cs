using Server.Models;

namespace Server.Data;

public interface IUserRepository
{
    Task<AppUser?> GetByUsername(string username);
    Task<AppUser?> GetByEmail(string email);
    Task<AppUser?> GetById(int id);
    Task<AppUser> Create(AppUser user);
}