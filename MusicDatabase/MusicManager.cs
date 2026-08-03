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
    async public Task<Track?> GetTrackAsync(Expression<Func<Track, bool>> filter)
    {
        return await _context.Tracks.AsNoTracking().FirstOrDefaultAsync(filter);
    }

    public IQueryable<Track> GetTracks()
    {
        return _context.Tracks.AsNoTracking();
    }

    async public Task AddTrackAsync(Track track)
    {
        await _context.Tracks.AddAsync(track);
        Logging.Success($"Track ({track.Id}) added.");
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

    async public Task<bool> RemoveTrackAsync(Expression<Func<Track, bool>> filter)
    {
        var trackData = await _context.Tracks
            .Where(filter)
                .Select(
                t => new
                {
                    t.Id,
                    t.Title,
                    AlbumId = t.Album.Id,
                    ArtistsIDs = t.Others.Select(a => a.Id),
                    ArtistIDs = t.Artist.Id
                }
            ).FirstOrDefaultAsync();
        if (trackData != null)
        {
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
                await _context.Artists
                    .Where(a => a.Id == trackData.ArtistIDs)
                    .Where(a => a.Tracks.Count == 0)
                    .ExecuteDeleteAsync();
                if (await _context.Tracks.CountAsync(t => t.AlbumId == trackData.AlbumId) == 0)
                    await RemoveAlbumAsync(a => a.Id == trackData.AlbumId);
                await transaciton.CommitAsync();
            }
            catch
            {
                await transaciton.RollbackAsync();
            }
            return true;
        }
        return false;
    }

    // USER
    public IQueryable<User> GetUsers()
    {
        return _context.Users.AsNoTracking();
    }

    public async Task<User?> GetUserAsync(Expression<Func<User, bool>> filter)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(filter);
        return user;
    }

    async public Task<bool> RemoveUserAsync(Expression<Func<User, bool>> filter)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(filter);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    async public Task<bool> AuthenticateUser(string name, string password)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Name == name);
        if (user == null) return false;
        return user.Password.Equals(password);
    }

    async public Task<bool> HasUserAsync(string name)
    {
        return await _context.Users.AnyAsync(u => u.Name == name);
    }

    async public Task<bool> AddUserAsync(string name, UserRole role, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Name == name))
            return false;
        
        User user = new() {Name = name, Password = password, Role = role};
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
    }

    // ALBUM
    async public Task<bool> RemoveAlbumAsync(Expression<Func<Album, bool>> filter)
    {
        Album? album = await _context.Albums
            .Include(a => a.Artist)
            .ThenInclude(a => a.Albums)
            .FirstOrDefaultAsync(filter);

        if (album != null)
        {
            _context.Albums.Remove(album);
            if (!album.Artist.Albums.Any(a => a != album))
                await RemoveArtistAsync((a) => a == album.Artist);
            return true;
        }
        return false;
    }

    public IQueryable<Album> GetAlbums()
    {
        return _context.Albums.AsNoTracking();
    }

    public async Task<Album?> GetAlbumAsync(Expression<Func<Album, bool>> filter)
    {
        return await _context.Albums.AsNoTracking().Include(a => a.Artist).Include(a => a.Tracks).FirstOrDefaultAsync(filter);
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
    public IQueryable<Artist> GetArtists()
    {
        return _context.Artists.AsNoTracking();
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

    async public Task<bool> RemoveArtistAsync(Expression<Func<Artist, bool>> filter)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(filter);

        if (artist != null)
        {
            _context.Artists.Remove(artist);
            return true;
        }
        return false;
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