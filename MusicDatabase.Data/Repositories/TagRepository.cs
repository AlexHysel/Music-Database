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

    public async Task<Tag> EnsureTagCreatedAsync(string tagName)
    {
        tagName = tagName.Trim().ToLowerInvariant();
        ArgumentException.ThrowIfNullOrEmpty(tagName);

        Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
        if (tag == null)
        {
            tag = new(tagName);
            await _context.Tags.AddAsync(tag);
        }
        return tag;
    }

    public async Task<bool> VoteForTrackTagAsync(Guid trackId, Tag tag, User user)
    {
        TrackTagStats? stats = await _context.TrackTagStats
            .FirstOrDefaultAsync(t => t.TrackId == trackId && t.TagId == tag.Id);
        
        if (stats == null)
        {
            stats = new(trackId, tag.Id);
            await _context.TrackTagStats.AddAsync(stats);
            stats.AddVote(user);
            return true;
        }

        if (await _context.AlbumTagStats.AnyAsync(t => t.Id == stats.Id && t.VotedBy.Any(u => u.Id == user.Id)))
            return false;
        else
        {
            stats.AddVote(user);
            return true;
        }
    }
    
    public async Task<bool> VoteForAlbumTagAsync(Guid albumId, Tag tag, User user)
    {
        AlbumTagStats? stats = await _context.AlbumTagStats
            .FirstOrDefaultAsync(t => t.AlbumId == albumId && t.TagId == tag.Id);
        
        if (stats == null)
        {
            stats = new(albumId, tag.Id);
            await _context.AlbumTagStats.AddAsync(stats);
            stats.AddVote(user);
            return true;
        }

        if (await _context.AlbumTagStats.AnyAsync(t => t.Id == stats.Id && t.VotedBy.Any(u => u.Id == user.Id)))
            return false;
        else
        {
            stats.AddVote(user);
            return true;
        }
    }

    public async Task<List<AlbumTagStats>> GetAlbumTagStatsAsync(Guid albumId, int numberOfTags)
    {
        var tags = await _context.AlbumTagStats
            .Where(s => s.AlbumId == albumId)
            .OrderByDescending(s => s.VoteCount)
            .Take(numberOfTags)
            .ToListAsync();
        
        return tags;
    }

    public async Task<List<TrackTagStats>> GetTrackTagStatsAsync(Guid trackId, int numberOfTags)
    {
        var tags = await _context.TrackTagStats
            .Where(s => s.TrackId == trackId)
            .OrderByDescending(s => s.VoteCount)
            .Take(numberOfTags)
            .ToListAsync();
        
        return tags;
    }
    
    public async Task<List<Tag>> GetUserAlbumTagsAsync(Guid albumId, User user)
    {
        List<Tag> tags = await _context.AlbumTagStats
            .Where(s => s.AlbumId == albumId && s.VotedBy.Contains(user))
            .OrderByDescending(s => s.VoteCount)
            .Select(s => s.Tag)
            .ToListAsync();
        
        return tags;
    }

    public async Task<List<Tag>> GetUserTrackTagsAsync(Guid trackId, User user)
    {
        List<Tag> tags = await _context.TrackTagStats
            .Where(s => s.TrackId == trackId && s.VotedBy.Contains(user))
            .OrderByDescending(s => s.VoteCount)
            .Select(s => s.Tag)
            .ToListAsync();
        
        return tags;
    }
}