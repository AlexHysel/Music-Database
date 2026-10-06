using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Data;

public class TrackRepository
{
    private readonly MusicDb _context;

    internal TrackRepository(MusicDb context)
    {
        _context = context;
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

    async public Task<Track?> GetTrackedTrackAsync(Guid id)
    {
        return await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackDetailAsync(Guid id){
        return await _context.Tracks.AsNoTracking()
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackedTrackDetailAsync(Guid id){
        return await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<bool> RemoveTrackAsync(Track track)
    {
        _context.Tracks.Remove(track);
        return true;
    }
}