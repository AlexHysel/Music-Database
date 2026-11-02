using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data.Repositories;

public class UserRepository
{
    private readonly MusicDb _context;

    internal UserRepository(MusicDb context)
    {
        _context = context;
    }

    public async Task<PagedResult<User>> GetMatchingUsersAsync(string name, int toSkip, int toTake){
        return await _context.Users
            .AsNoTracking()
            .Where(t => EF.Functions.ILike(t.Name, $"%{name}%"))
            .ToPagedResultAsync(toSkip, toTake);
    }

    public async Task<User?> GetUserAsync(Guid id, bool tracked = false){
        if (tracked)
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        else
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserAsync(string name, bool tracked = false){
        if (tracked)
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        else
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<User?> GetUserDetailAsync(Guid id){
        return await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    async public Task<bool> RemoveUserAsync(Guid id){
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            _context.Users.Remove(user);
            return true;
        }
        return false;
    }

    async public Task<bool> CreateUserAsync(string name, string passwordHash)
    {
        if (await _context.Users.AnyAsync(u => u.Name == name))
            return false;

        var user = User.CreatePersisted(name, passwordHash);
        await _context.Users.AddAsync(user);
        return true;
    }
}