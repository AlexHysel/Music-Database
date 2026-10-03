using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data;

public class AlbumRepository
{
    private readonly MusicDb _context;

    internal AlbumRepository(MusicDb context)
    {
        _context = context;
    }

    async public Task<PagedResult<Album>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake)
    {
        return await _context.Albums
            .Where(t => EF.Functions.ILike(t.Title, $"%{title}%"))
            .OrderBy(a => a.Title)
            .ThenBy(a => a.Id)
            .ToPagedResultAsync(toSkip, toTake);
    }

    async public Task<bool> CreateAlbumAsync(Album album)
    {
        await _context.Albums.AddAsync(album);
        return true;
    }

    async public Task<bool> RemoveAlbumAsync(Guid id)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == id);
        if (album != null)
        {
            _context.Albums.Remove(album);
            return true;
        }
        return false;
    }

    async public Task<Album?> GetTrackedAlbumAsync(Guid id)
    {
        return await _context.Albums.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        Album? album = await _context.Albums.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        return album;
    }

    public async Task<Album?> GetAlbumDetailAsync(Guid id)
    {
        Album? album = await _context.Albums.AsNoTracking()
            .Include(a => a.Artist)
            .Include(a => a.Tracks).ThenInclude(t => t.Artist)
            .Include(a => a.Tracks).ThenInclude(t => t.Others)
            .FirstOrDefaultAsync(a => a.Id == id);
        return album;
    }

    public async Task<Album> EnsureAlbumCreated(string title, Artist artist)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Artist.Id == artist.Id && a.Title == title);
        if (album == null)
        {
            //temporary album with release year 0, will be updated later
            album = new(title, 0, artist);
            await _context.Albums.AddAsync(album);
        }
        return album;
    }
}