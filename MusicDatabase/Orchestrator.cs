// BUSINESS LOGIC LAYER
using Microsoft.EntityFrameworkCore;

class Orchestrator
{
    private readonly MusicManager _manager;

    public Orchestrator(MusicManager manager) => _manager = manager;

    public async Task AddTrackAsync()
    {
        Console.Write("Title: ");
        string title = Console.ReadLine().Trim();

        Console.Write("Album Artist Name: ");
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

    public async Task AddUserAsync()
    {
        Console.Write("Name: ");
        string name = Console.ReadLine().Trim();
        Console.Write("Password: ");
        string password = Console.ReadLine().Trim();

        await _manager.AddUserAsync(name, password);
    }

    public async Task DisplayTracksAsync()
    {
        await _manager.DisplayTracksAsync();
    }

    public async Task DisplayAlbumsAsync()
    {
        await _manager.DisplayAlbumsAsync();
    }

    public async Task DisplayArtistsAsync()
    {
        try 
        {
            Console.Write("Page size: ");
            int size = Convert.ToInt32(Console.ReadLine().Trim());
            Console.Write("Page number: ");
            int page = Convert.ToInt32(Console.ReadLine().Trim());
            IQueryable<ArtistDTO> request = _manager.GetArtists();
            var artists = await request.Skip((page - 1) * size).Take(size).ToArrayAsync();
            foreach (var artist in artists)
                Console.WriteLine($"{artist.Name} ({artist.Id})");
        }
        catch
        {
            Logging.Error("Page number and size should be numbers");
        }
    }

    public async Task DisplayUsersAsync()
    {
        await _manager.DisplayUsersAsync();
    }

    public async Task RemoveTrackAsync()
    {
        Console.Write("Title: ");
        string title = Console.ReadLine().Trim();
        Console.Write("Album Title: ");
        string albumTitle = Console.ReadLine().Trim();

        await _manager.RemoveTrackAsync(t => t.Album.Title == albumTitle && t.Title == title);
        await _manager.SaveChangesAsync();
    }

    public async Task RemoveAlbumAsync()
    {
        Console.Write("Title: ");
        string title = Console.ReadLine().Trim();
        Console.Write("Artist: ");
        string artistName = Console.ReadLine().Trim();

        await _manager.RemoveAlbumAsync(a => a.Artist.Name == artistName && a.Title == title);
        await _manager.SaveChangesAsync();
    }

    public async Task RemoveArtistAsync()
    {
        Console.Write("Name: ");
        string name = Console.ReadLine().Trim();

        await _manager.RemoveArtistAsync(a => a.Name == name);
        await _manager.SaveChangesAsync();
    }

    public async Task RemoveUserAsync()
    {
        Console.Write("Name: ");
        string name = Console.ReadLine().Trim();

        await _manager.RemoveUserAsync(u => u.Name == name);
        await _manager.SaveChangesAsync();
    }
}