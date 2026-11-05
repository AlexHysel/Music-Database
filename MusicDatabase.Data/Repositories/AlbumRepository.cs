using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data.Repositories;

public class AlbumRepository
{
    private readonly MusicDb _context;

    internal AlbumRepository(MusicDb context)
    {
        _context = context;
    }

    async public Task<bool> AlbumExistsAsync(Guid id, CancellationToken ctg = default){
        return await _context.Albums.AnyAsync(a => a.Id == id, ctg);
    }

    async public Task<PagedResult<Album>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake, CancellationToken ctg = default)
    {
        return await _context.Albums
            .AsNoTracking()
            .Where(t => EF.Functions.ILike(t.Title, $"%{title}%"))
            .Include(a => a.Artist)
            .OrderBy(a => a.Title)
            .ThenBy(a => a.Id)
            .ToPagedResultAsync(toSkip, toTake, ctg);
    }

    async public Task<bool> CreateAlbumAsync(Album album, CancellationToken ctg = default)
    {
        await _context.Albums.AddAsync(album, ctg);
        return true;
    }

    async public Task<bool> RemoveAlbumAsync(Guid id, CancellationToken ctg = default)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == id, ctg);
        if (album != null)
        {
            _context.Albums.Remove(album);
            return true;
        }
        return false;
    }

    public async Task<Album?> GetAlbumAsync(Guid id, CancellationToken ctg = default)
    {
        Album? album = await _context.Albums.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ctg);
        return album;
    }

    public async Task<Album?> GetAlbumDetailAsync(Guid id, bool tracked = false, CancellationToken ctg = default)
    {
        IQueryable<Album> query = _context.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks).ThenInclude(t => t.Artist)
            .Include(a => a.Tracks).ThenInclude(t => t.Others);
        if (!tracked) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(a => a.Id == id, ctg);
    }
}