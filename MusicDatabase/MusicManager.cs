using Microsoft.EntityFrameworkCore;

class MusicManager
{
    private readonly MusicDb Context;

    public MusicManager(MusicDb context) => Context = context;

    async public Task AddTrackAsync(Track track)
    {
        await Context.Tracks.AddAsync(track);
        await Context.SaveChangesAsync();
    }

    async public Task DisplayTracksAsync()
    {
        var tracks = Context.Tracks.AsNoTracking().Select(x => new
        {
            x.Title,
            AlbumTitle = x.Album.Title,
            ArtistsNames = x.Artists.Select(x => x.Name).ToArray()
        });
        foreach (var track in tracks)
        {
            string artists = "";
            foreach (string artist in track.ArtistsNames)
                artists = artists + ", " + artist;
            Console.WriteLine($"{track.Title} ({track.AlbumTitle}) by {track.ArtistsNames}");
        }
    }

    async public Task EditTrackAsync(Guid trackId, string? newTitle = null, Genre? newGenre = null)
    {
        Track trackChanges = new() { Id = trackId};
        Context.Attach(trackChanges);
        if (newTitle != null)
            trackChanges.Title = newTitle;
        if (newGenre != null)
            trackChanges.Genre = newGenre.Value;
        await Context.SaveChangesAsync();
    }

    async public Task RemoveTrackAsync(Guid id)
    {
        Track? track = await Context.Tracks.FirstOrDefaultAsync(t => t.Id == id);
        if (track != null)
        {
            Context.Tracks.Remove(track);
            await Context.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine($"[ERROR] Cannot remove track with id {id} because it's not found");
        }
    }

    async public Task RemoveTrackAsync(Track track)
    {
        if (await Context.Tracks.ContainsAsync(track))
        {
            Context.Tracks.Remove(track);
            await Context.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine($"[ERROR] Cannot remove track with id {track.Id} because it's not found");
        }
    }
}