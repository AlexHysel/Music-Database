using Microsoft.EntityFrameworkCore;

class Orchestrator
{
    private readonly MusicManager _manager;

    public Orchestrator(MusicManager manager) => _manager = manager;

    public async Task AddTrack()
    {
        Console.Write("Title: ");
        string title = Console.ReadLine().Trim();

        Console.Write("Main Artist Name: ");
        string artistName = Console.ReadLine().Trim();

        Console.Write("Other Artists Name: ");
        string? others = Console.ReadLine().Trim();
        List<Artist> artists = [await _manager.EnsureArtistCreated(artistName)];
        if (!string.IsNullOrEmpty(others))
            foreach (string name in others.Split(','))
                artists.Add(await _manager.EnsureArtistCreated(name));

        Console.Write("Album Title: ");
        string albumTitle = Console.ReadLine().Trim();
        Album album = await _manager.EnsureAlbumCreated(albumTitle, artists[0]);
        
        Track track = new() {Title = title, Album = album, Artists = artists, Genre = Genre.DARKSYNTH};
        await _manager.AddTrackAsync(track);
        await _manager.SaveChangesAsync();
    }
}