using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

// DATA ACCESS LAYER
public class MusicManager
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
    async public Task<bool> TrackExistsAsync(Guid id)
    {
        return await _context.Tracks.AnyAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackAsync(Guid id)
    {
        return await _context.Tracks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public IQueryable<Track> GetTracks()
    {
        return _context.Tracks.AsNoTracking();
    }

    async public Task AddTrackAsync(Track track)
    {
        await _context.Tracks.AddAsync(track);
    }

    async public Task<bool> AddTrackToFavoritesAsync(Track track, User user)
    {
        if (!user.FavoriteTracks.Contains(track))
        {
            user.FavoriteTracks.Add(track);
            return true;
        }
        return false;
    }

    async public Task<bool> UpdateTrackAsync(Track track)
    {
        Track? existing = await _context.Tracks
            .Include(t => t.Others)
            .FirstOrDefaultAsync(t => t.Id == track.Id);
        if (existing == null) return false;
        
        existing.Title = track.Title;
        existing.Genre = track.Genre;
        existing.Album = track.Album;
        existing.Artist = track.Artist;
        existing.Others.Clear();
        foreach (var other in track.Others)
            existing.Others.Add(other);
        
        return true;
    }

    async public Task<bool> RemoveTrackAsync(Guid id)
    {
        Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id);
        if (track != null)
        {
            _context.Tracks.Remove(track);
            return true;
        }
        return false;
    }

    // USER
    public IQueryable<User> GetUsers()
    {
        return _context.Users.AsNoTracking();
    }

    public async Task<User?> GetUserAsync(Guid id)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user;
    }

    async public Task<bool> RemoveUserAsync(Guid id)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            _context.Users.Remove(user);
            return true;
        }
        return false;
    }

    async public Task<User?> AuthenticateUser(string name, string password)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Name == name);
        if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
        {
            return user;
        }
        else
        {
            return null;
        }
    }

    async public Task<bool> UserExistsAsync(string name)
    {
        return await _context.Users.AnyAsync(u => u.Name == name);
    }

    async public Task<bool> UserExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    async public Task<bool> AddUserAsync(string name, UserRole role, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Name == name))
            return false;
        
        User user = new() {Name = name, Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password), Role = role};
        await _context.Users.AddAsync(user);
        return true;
    }

    // ALBUM
    async public Task<bool> RemoveAlbumAsync(Guid id)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == id);
        if (album != null)
        {
            _context.Albums.Remove(album);
            return true;
        }
        return false;
    }

    public IQueryable<Album> GetAlbums()
    {
        return _context.Albums.AsNoTracking();
    }

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        return await _context.Albums.AsNoTracking().Include(a => a.Artist).Include(a => a.Tracks).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> AddAlbumToFavoritesAsync(Album album, User user)
    {
        if (!user.FavoriteAlbums.Contains(album))
        {
            user.FavoriteAlbums.Add(album);
            return true;
        }
        return false;
    }

    public async Task AddAlbumAsync(Album album)
    {
        if (album.Artist.Albums.FirstOrDefault(a => a.Title == album.Title) == null)
        {
            await _context.Albums.AddAsync(album);
        }
    }

    public async Task<Album> EnsureAlbumCreated(string title, Artist artist)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Artist.Id == artist.Id && a.Title == title);
        if (album == null)
        {
            album = new() {Title = title, Artist = artist, Type = AlbumType.Single};
            await _context.Albums.AddAsync(album);
        }
        return album;
    }

    public async Task<bool> AlbumExistsAsync(Guid id)
    {
        return await _context.Albums.AnyAsync(a => a.Id == id);
    }

    // ARTIST
    public async Task<bool> ArtistExistsAsync(Guid id)
    {
        return await _context.Artists.AnyAsync(a => a.Id == id);
    }

    public IQueryable<Artist> GetArtists()
    {
        return _context.Artists.AsNoTracking();
    }

    public async Task<bool> AddArtistToFavoritesAsync(Artist artist, User user)
    {
        if (!user.FavoriteArtists.Contains(artist))
        {
            user.FavoriteArtists.Add(artist);
            return true;
        }
        return false;
    }

    public async Task<Artist> EnsureArtistCreated(string name)
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

    async public Task<bool> RemoveArtistAsync(Guid id)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == id);

        if (artist != null)
        {
            _context.Artists.Remove(artist);
            return true;
        }
        return false;
    }

    async public Task<bool> UpdateArtistAsync(Artist patch)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == patch.Id);
        if (artist != null)
        {
            artist.Name = patch.Name;
            return true;
        }
        return false;
    }

    async public Task AddArtistAsync(Artist artist)
    {
        if (!await _context.Artists.AnyAsync(a => a.Name == artist.Name))
        {
            await _context.Artists.AddAsync(artist);
        }
    }
}