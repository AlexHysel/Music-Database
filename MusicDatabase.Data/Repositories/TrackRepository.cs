using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data.Repositories;

public class TrackRepository
{
    private readonly MusicDb _context;

    internal TrackRepository(MusicDb context)
    {
        _context = context;
    }

    async public Task<bool> AddTagOnTrackAsync(TrackTagVote trackTag)
    {
        if (await _context.TrackTagVotes.AnyAsync(t => t == trackTag))
            return false;

        await _context.TrackTagVotes.AddAsync(trackTag);
        return true;
    }

    async public Task<PagedResult<Track>> GetArtistTracksAsync(Guid artistId, int toSkip, int toTake)
    {
        return await _context.Tracks.AsNoTracking()
            .Where(t => t.ArtistId == artistId)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.Id)
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .ToPagedResultAsync(toSkip, toTake);
    }

    async public Task<PagedResult<Track>> GetMatchingTracksAsync(string title, int toSkip, int toTake){
        return await _context.Tracks.AsNoTracking()
            .Where(t => EF.Functions.ILike(t.Title, $"%{title}%"))
            .OrderBy(t => t.Title)
            .ThenBy(t => t.Id)
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .ToPagedResultAsync(toSkip, toTake);
    }

    async public Task<Track?> GetTrackAsync(Guid id)
    {
        return await _context.Tracks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackDetailAsync(Guid id, bool tracked = false){
        IQueryable<Track> query = _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others);
        if (!tracked) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<string[]> GetTrackTags(Guid id, int count)
    {
        var ids = await _context.TrackTagVotes
            .AsNoTracking()
            .Where(v => v.TrackId == id)
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

    async public void RemoveTrack(Track track)
    {
        _context.Tracks.Remove(track);
    }
}