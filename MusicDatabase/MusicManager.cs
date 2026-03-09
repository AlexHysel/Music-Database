using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

// DATA ACCESS LAYER
class MusicManager
{
    private readonly MusicDb _context;

    public MusicManager(MusicDb context) => _context = context;

    async public Task SaveChangesAsync()
    {
        var updatedAlbums = _context.ChangeTracker.Entries<Album>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        await _context.SaveChangesAsync();
    }

    // TRACK
    async public Task<Track?> GetTrackAsync(Expression<Func<Track, bool>> filter)
    {
        return await _context.Tracks.FirstOrDefaultAsync(filter);
    }

    public IQueryable<TrackDTO> GetTracks()
    {
        return _context.Tracks.Select(t => 
        new TrackDTO(t.Title, t.Album.Title, t.Artists.Select(a => a.Name).ToArray(), t.Genre.ToString()));
    }

    public IQueryable<TrackDTO> GetTracks(Expression<Func<Track, bool>> filter)
    {
        return _context.Tracks.Where(filter).Select(t => 
        new TrackDTO(t.Title, t.Album.Title, t.Artists.Select(a => a.Name).ToArray(), t.Genre.ToString()));
    }

    async public Task AddTrackAsync(Track track)
    {
        await _context.Tracks.AddAsync(track);
        Logging.Success($"Track ({track.Id}) added.");
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
        var trackData = await _context.Tracks
            .Where(filter)
                .Select(
                t => new
                {
                    t.Id,
                    t.Title,
                    AlbumId = t.Album.Id,
                    ArtistsIDs = t.Artists.Select(a => a.Id)
                }
            ).FirstOrDefaultAsync();

        if (trackData == null)
        {
            Logging.Warning("Track not found");
            return;
        }
        
        using var transaciton = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Tracks
                .Where(t => t.Id == trackData.Id)
                .ExecuteDeleteAsync();
            await _context.Artists
                .Where(a => trackData.ArtistsIDs.Contains(a.Id))
                .Where(a => a.Tracks.Count == 0)
                .ExecuteDeleteAsync();
            if (await _context.Tracks.CountAsync(t => t.AlbumId == trackData.AlbumId) == 0)
                await RemoveAlbumAsync(a => a.Id == trackData.AlbumId);
            Logging.Success($"{trackData.Title} ({trackData.Id}) removed.");
            await transaciton.CommitAsync();
        }
        catch
        {
            await transaciton.RollbackAsync();
            Logging.Error("Removing failed.");
        }
    }

    // USER
    public IQueryable<UserDTO> GetUsers()
    {
        return _context.Users.Select(u => new UserDTO(u.Name, u.Id.ToString()));
    }

    public IQueryable<UserDTO> GetUsers(Expression<Func<User, bool>> filter)
    {
        return _context.Users.Where(filter).Select(u => new UserDTO(u.Name, u.Id.ToString()));
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
    async public Task RemoveAlbumAsync(Expression<Func<Album, bool>> filter)
    {
        Album? album = await _context.Albums.Include(a => a.Artist).ThenInclude(a => a.Albums).FirstOrDefaultAsync(filter);

        if (album == null)
            Logging.Warning("Album not found.");
        else
        {
            _context.Albums.Remove(album);
            Logging.Success($"{album.Title} ({album.Id}) removed.");
            if (!album.Artist.Albums.Any(a => a != album))
                await RemoveArtistAsync((a) => a == album.Artist);
        }
    }

    public IQueryable<AlbumDTO> GetAlbums()
    {
        return _context.Albums.Select(a => new AlbumDTO(a.Title, a.Artist.Name, a.Type.ToString()));
    }

    public IQueryable<AlbumDTO> GetAlbums(Expression<Func<Album, bool>> filter)
    {
        return _context.Albums.Where(filter).Select(a => new AlbumDTO(a.Title, a.Artist.Name, a.Type.ToString()));
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
            album = new() {Title = title, Artist = artist, Type = AlbumType.Single};
            await _context.Albums.AddAsync(album);
        }
        return album;
    }

    // ARTIST
    public IQueryable<ArtistDTO> GetArtists()
    {
        return _context.Artists.Select(a => new ArtistDTO(a.Name, a.Id.ToString()));
    }

    public IQueryable<ArtistDTO> GetArtists(Expression<Func<Artist, bool>> filter)
    {
        return _context.Artists.Where(filter).Select(a => new ArtistDTO(a.Name, a.Id.ToString()));
    }

    async public Task<Artist> EnsureArtistCreated(string name)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name);
        if (artist == null)
            artist = _context.Artists.Local.FirstOrDefault(a => a.Name == name);
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