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

    async public Task<bool> AddTagOnAlbumAsync(AlbumTagVote albumTag)
    {
        if (await _context.AlbumTagVotes.AnyAsync(t => t == albumTag))
            return false;

        await _context.AlbumTagVotes.AddAsync(albumTag);
        return true;
    }

    public async Task<string[]> GetAlbumTags(Guid id, int count)
    {
        var ids = await _context.AlbumTagVotes
            .Where(v => v.AlbumId == id)
            .GroupBy(v => v.TagId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync();

        var tags = await _context.Tags
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.Name)
            .ToArrayAsync();

        return tags;
    }

    async public Task<PagedResult<Album>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake)
    {
        return await _context.Albums
            .AsNoTracking()
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

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        Album? album = await _context.Albums.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        return album;
    }

    public async Task<Album?> GetAlbumDetailAsync(Guid id, bool tracked = false)
    {
        IQueryable<Album> query = _context.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks).ThenInclude(t => t.Artist)
            .Include(a => a.Tracks).ThenInclude(t => t.Others);
        if (!tracked) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(a => a.Id == id);
    }
}