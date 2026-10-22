using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data.Repositories;

public class ArtistRepository
{
    private readonly MusicDb _context;

    internal ArtistRepository(MusicDb context)
    {
        _context = context;
    }

    public async Task<bool> ArtistExistsAsync(Guid id)
    {
        return await _context.Artists.AnyAsync(a => a.Id == id);
    }
    
    public async Task<bool> ArtistExistsAsync(string name)
    {
        return await _context.Artists.AnyAsync(a => a.Name == name);
    }

    public async Task<PagedResult<Artist>> GetMatchingArtistsAsync(string name, int toSkip, int toTake){
        return await _context.Artists
            .AsNoTracking()
            .Where(t => EF.Functions.ILike(t.Name, $"%{name}%"))
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .ToPagedResultAsync(toSkip, toTake);
    }

    public async Task<Artist?> GetArtistAsync(Guid id, bool tracked = false){
        if (tracked)
            return await _context.Artists.FirstOrDefaultAsync(a => a.Id == id);
        else
            return await _context.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artist?> GetArtistDetailAsync(Guid id){
        return await _context.Artists.AsNoTrackingWithIdentityResolution()
            .Include(a => a.Albums)
            .Include(a => a.Tracks).ThenInclude(t => t.Album)
            .Include(a => a.Tracks).ThenInclude(t => t.Others)
            .Include(a => a.AppearsOn).ThenInclude(t => t.Album)
            .Include(a => a.AppearsOn).ThenInclude(t => t.Artist)
            .Include(a => a.AppearsOn).ThenInclude(t => t.Others)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Artist> EnsureArtistCreated(string name)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name);
        if (artist == null)
            artist = _context.Artists.Local.FirstOrDefault(a => a.Name == name);
        if (artist == null)
        {
            artist = new Artist(name);
            await _context.Artists.AddAsync(artist);
        }
        return artist;
    }

    async public Task<bool> RemoveArtistAsync(Guid id)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == id);

        if (artist != null)
        {
            _context.Artists.Remove(artist);
            return true;
        }
        return false;
    }
}