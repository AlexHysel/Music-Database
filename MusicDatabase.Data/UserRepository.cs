using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data;

public class UserRepository
{
    private readonly MusicDb _context;

    internal UserRepository(MusicDb context)
    {
        _context = context;
    }

    public async Task<PagedResult<User>> GetMatchingUsersAsync(string name, int toSkip, int toTake){
        return await _context.Users
            .Where(t => EF.Functions.ILike(t.Name, $"%{name}%"))
            .ToPagedResultAsync(toSkip, toTake);
    }

    public async Task<User?> GetUserAsync(Guid id){
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetTrackedUserAsync(Guid id){
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserDetailAsync(Guid id){
        return await _context.Users.AsNoTracking()
            .Include(u => u.FavoriteArtists)
            .Include(u => u.FavoriteAlbums)
            .Include(u => u.FavoriteTracks)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<User?> GetTrackedUserDetailAsync(Guid id){
        return await _context.Users
            .Include(u => u.FavoriteArtists)
            .Include(u => u.FavoriteAlbums)
            .Include(u => u.FavoriteTracks)
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

    async public Task<User?> AuthenticateUser(string name, string password)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Name == name);
        if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
            return user;
        else
            return null;
    }

    async public Task<bool> CreateUserAsync(string name, UserRole role, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Name == name))
            return false;
        
        User user = new(name, BCrypt.Net.BCrypt.EnhancedHashPassword(password), role);
        await _context.Users.AddAsync(user);
        return true;
    }
}