using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

class MusicManager
{
    private readonly MusicDb _context;

    public MusicManager(MusicDb context) => _context = context;

    async public Task SaveChangesAsync() => await _context.SaveChangesAsync();

    // TRACK
    async public Task<Track?> GetTrackAsync(Expression<Func<Track, bool>> filter)
    {
        return await _context.Tracks.FirstOrDefaultAsync(filter);
    }

    async public Task AddTrackAsync(Track track)
    {
        await _context.Tracks.AddAsync(track);
        Logging.Success($"Track ({track.Id}) added.");
    }

    async public Task DisplayTracksAsync()
    {
        Console.WriteLine("=== TRACKS ===");

        var tracks = _context.Tracks.AsNoTracking().Select(x => new
        {
            x.Title,
            AlbumTitle = x.Album.Title,
            ArtistsNames = x.Artists.Select(x => x.Name).ToArray()
        });
        foreach (var track in tracks)
        {
            string artists = track.ArtistsNames[0];
            for (int i = 1; i < track.ArtistsNames.Count(); i++)
                artists = artists + ", " + track.ArtistsNames[i];
            Console.WriteLine($"{track.Title} ({track.AlbumTitle}) by {artists}");
        }
        Console.WriteLine();
    }

    async public Task EditTrackAsync(Guid trackId, string? newTitle = null, Genre? newGenre = null)
    {
        Track trackChanges = new() { Id = trackId};
        _context.Attach(trackChanges);
        if (newTitle != null)
            trackChanges.Title = newTitle;
        if (newGenre != null)
            trackChanges.Genre = newGenre.Value;
        await _context.SaveChangesAsync();
    }

    async public Task RemoveTrackAsync(Expression<Func<Track, bool>> filter)
    {
        Track? track = await _context.Tracks.Include(t => t.Album).ThenInclude(a => a.Tracks).FirstOrDefaultAsync(filter);

        if (track == null)
            Logging.Warning("Track not found");
        else
        {
            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();
            Logging.Success($"{track.Title} ({track.Id}) removed.");
            if (!track.Album.Tracks.Any())
                await RemoveAlbumAsync((a) => a == track.Album);
        }
    }

    // USER
    async public Task DisplayUsersAsync()
    {
        Console.WriteLine("=== USERS ===");

        string[] users = await _context.Users.AsNoTracking().Select(u => u.Name).ToArrayAsync();
        foreach (string name in users)
            Console.WriteLine(name);
        Console.WriteLine();
    }

    async public Task RemoveUserAsync(Expression<Func<User, bool>> filter)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(filter);
        if (user == null)
            Logging.Warning("User not found.");
        else
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            Logging.Success($"{user.Name} ({user.Id}) removed.");
        }
    }

    async public Task AddUserAsync(string name, string password)
    {
        User user = new() {Name = name, Password = password};
        await _context.Users.AddAsync(user);
        try
        {
            await _context.SaveChangesAsync();
            Logging.Success($"User {name} created.");
        } 
        catch
        {
            Logging.Error($"User {name} already exists.");
        }
    }

    // ALBUM
    async public Task DisplayAlbumsAsync()
    {
        Console.WriteLine("=== ALBUMS ===");

        var albums = await _context.Albums.AsNoTracking().Select(a => new
        {
            a.Title,
            ArtistName = a.Artist.Name,
            a.Type,
        }).ToArrayAsync();
        foreach (var album in albums)
            Console.WriteLine($"{album.Title} ({album.Type}) by {album.ArtistName}");
    }

    async public Task RemoveAlbumAsync(Expression<Func<Album, bool>> filter)
    {
        Album? album = await _context.Albums.Include(a => a.Artist).ThenInclude(a => a.Albums).FirstOrDefaultAsync(filter);

        if (album == null)
            Logging.Warning("Album not found.");
        else
        {
            _context.Albums.Remove(album);
            Logging.Success($"{album.Title} ({album.Id}) removed.");
            if (!album.Artist.Albums.Any())
                await RemoveArtistAsync((a) => a == album.Artist);
        }
    }

    async public Task AddAlbumAsync(Album album)
    {
        if (album.Artist.Albums.FirstOrDefault(a => a.Title == album.Title) == null)
        {
            await _context.Albums.AddAsync(album);
            Logging.Success($"Album {album.Title} added.");
        }
        else
            Logging.Error("Artist already has album with this title.");
    }

    async public Task<Album> EnsureAlbumCreated(string title, Artist artist)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Artist.Id == artist.Id && a.Title == title);
        if (album == null)
        {
            album = new() {Title = title, Artist = artist};
            await _context.Albums.AddAsync(album);
        }
        return album;
    }

    // ARTIST
    async public Task DisplayArtistAsync()
    {
        Console.WriteLine("=== ARTISTS ===");

        var artists = await _context.Artists.AsNoTracking().Select(a => new
        {
            a.Name,
            a.Id
        }).ToArrayAsync();
        foreach (var artist in artists)
            Console.WriteLine($"{artist.Name} ({artist.Id})");
    }

    async public Task<Artist> EnsureArtistCreated(string name)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name);
        if (artist == null)
        {
            artist = new() {Name = name};
            await _context.Artists.AddAsync(artist);
        }
        return artist;
    }

    async public Task RemoveArtistAsync(Expression<Func<Artist, bool>> filter)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(filter);

        if (artist == null)
            Logging.Warning("Artist not found.");
        else
        {
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            Logging.Success($"{artist.Name} ({artist.Id}) removed.");
        }
    }

    async public Task AddArtistAsync(Artist artist)
    {
        if (await _context.Artists.AnyAsync(a => a.Name == artist.Name))
            Logging.Error("This artist already exist.");
        else
        {
            await _context.Artists.AddAsync(artist);
            Logging.Success($"Artist {artist.Name} added.");
        }
    }
}