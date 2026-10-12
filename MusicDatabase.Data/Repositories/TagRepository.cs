using Microsoft.EntityFrameworkCore;

namespace MusicDatabase.Data.Repositories;

public class TagRepository
{
    private readonly MusicDb _context;

    internal TagRepository(MusicDb context)
    {
        _context = context;
    }

    public async Task<Tag> EnsureTagCreated(string tagName)
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
}