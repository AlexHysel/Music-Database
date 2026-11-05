using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;

namespace MusicDatabase.Data.Repositories;

public class TagRepository
{
    private readonly MusicDb _context;

    internal TagRepository(MusicDb context)
    {
        _context = context;
    }

    public async Task<bool> TagExistsAsync(Guid id, CancellationToken ctg = default)
    {
        return await _context.Tags.AnyAsync(t => t.Id == id, ctg);
    }
    
    public async Task<bool> TrackTagStatsExistsAsync(Guid id, CancellationToken ctg = default)
    {
        return await _context.TrackTagStats.AnyAsync(t => t.Id == id, ctg);
    }
    
    public async Task<bool> AlbumTagStatsExistsAsync(Guid id, CancellationToken ctg = default)
    {
        return await _context.AlbumTagStats.AnyAsync(t => t.Id == id, ctg);
    }

    public async Task<Tag> EnsureTagCreatedAsync(string tagName, CancellationToken ctg = default)
    {
        tagName = tagName.Trim().ToLowerInvariant();
        ArgumentException.ThrowIfNullOrEmpty(tagName);

        Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName, ctg);
        if (tag == null)
        {
            tag = new(tagName);
            await _context.Tags.AddAsync(tag, ctg);
        }
        return tag;
    }

    public async Task<bool> VoteForTrackTagAsync(Guid trackId, Tag tag, User user, CancellationToken ctg = default)
    {
        TrackTagStats? stats = _context.TrackTagStats.Local
            .FirstOrDefault(t => t.TrackId == trackId && t.TagId == tag.Id);

        if (stats == null)
        {
            stats = await _context.TrackTagStats
                .FirstOrDefaultAsync(t => t.TrackId == trackId && t.TagId == tag.Id, ctg);
        }

        if (stats == null)
        {
            stats = new(trackId, tag.Id);
            await _context.TrackTagStats.AddAsync(stats, ctg);
            stats.AddVote(user);
            return true;
        }

        if (stats.VotedBy.Any(u => u.Id == user.Id))
            return false;

        stats.AddVote(user);
        return true;
    }
    
    public async Task<bool> VoteForAlbumTagAsync(Guid albumId, Tag tag, User user, CancellationToken ctg = default)
    {
        AlbumTagStats? stats = _context.AlbumTagStats.Local
            .FirstOrDefault(t => t.AlbumId == albumId && t.TagId == tag.Id);

        if (stats == null)
        {
            stats = await _context.AlbumTagStats
                .FirstOrDefaultAsync(t => t.AlbumId == albumId && t.TagId == tag.Id, ctg);
        }

        if (stats == null)
        {
            stats = new(albumId, tag.Id);
            await _context.AlbumTagStats.AddAsync(stats, ctg);
            stats.AddVote(user);
            return true;
        }

        if (stats.VotedBy.Any(u => u.Id == user.Id))
            return false;

        stats.AddVote(user);
        return true;
    }

    public async Task<List<AlbumTagStats>> GetAlbumTagStatsAsync(Guid albumId, int numberOfTags, CancellationToken ctg = default)
    {
        var tags = await _context.AlbumTagStats
            .Where(s => s.AlbumId == albumId)
            .Include(s => s.Tag)
            .OrderByDescending(s => s.VoteCount)
            .Take(numberOfTags)
            .ToListAsync(ctg);
        
        return tags;
    }

    public async Task<bool> RemoveTrackTagVoteAsync(Guid trackId, Tag tag, User user, CancellationToken ctg = default)
    {
        var stats = await _context.TrackTagStats
            .Include(s => s.Tag)
            .Include(s => s.VotedBy)
            .FirstOrDefaultAsync(s => s.TrackId == trackId && s.TagId == tag.Id, ctg);

        if (stats == null)
            return false;

        if (!stats.RemoveVote(user))
            return false;

        return true;
    }

    public async Task<bool> RemoveAlbumTagVoteAsync(Guid albumId, Tag tag, User user, CancellationToken ctg = default)
    {
        var stats = await _context.AlbumTagStats
            .Include(s => s.Tag)
            .Include(s => s.VotedBy)
            .FirstOrDefaultAsync(s => s.AlbumId == albumId && s.TagId == tag.Id, ctg);

        if (stats == null)
            return false;

        if (!stats.RemoveVote(user))
            return false;

        return true;
    }

    public async Task<List<TrackTagStats>> GetTrackTagStatsAsync(Guid trackId, int numberOfTags, CancellationToken ctg = default)
    {
        var tags = await _context.TrackTagStats
            .Where(s => s.TrackId == trackId)
            .Include(s => s.Tag)
            .OrderByDescending(s => s.VoteCount)
            .Take(numberOfTags)
            .ToListAsync(ctg);
        
        return tags;
    }
    
    public async Task<List<Tag>> GetUserAlbumTagsAsync(Guid albumId, User user, CancellationToken ctg = default)
    {
        List<Tag> tags = await _context.AlbumTagStats
            .Where(s => s.AlbumId == albumId && s.VotedBy.Contains(user))
            .Include(s => s.Tag)
            .OrderByDescending(s => s.VoteCount)
            .Select(s => s.Tag)
            .ToListAsync(ctg);
        
        return tags;
    }

    public async Task<List<Tag>> GetUserTrackTagsAsync(Guid trackId, User user, CancellationToken ctg = default)
    {
        List<Tag> tags = await _context.TrackTagStats
            .Where(s => s.TrackId == trackId && s.VotedBy.Contains(user))
            .Include(s => s.Tag)
            .OrderByDescending(s => s.VoteCount)
            .Select(s => s.Tag)
            .ToListAsync(ctg);
        
        return tags;
    }
}