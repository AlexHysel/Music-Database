namespace MusicDatabase.Domain;

public class User
{
    public Guid Id {get; private set;}
    public string Name {get; private set;}
    public UserRole Role {get; private set;}
    public string Password {get; private set;}
    private readonly List<Track> _favoriteTracks = new();
    public IReadOnlyCollection<Track> FavoriteTracks => _favoriteTracks.AsReadOnly();
    private readonly List<Album> _favoriteAlbums = new();
    public IReadOnlyCollection<Album> FavoriteAlbums => _favoriteAlbums.AsReadOnly();
    private readonly List<Artist> _favoriteArtists = new();
    public IReadOnlyCollection<Artist> FavoriteArtists => _favoriteArtists.AsReadOnly();
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public User(string name, string password, UserRole role)
    {
        name = name.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.NewGuid();
        Name = name;
        Role = role;
        Password = password;
    }

    public bool SetName(string name)
    {
        name = name.Trim();
        if (name.Length < 1) return false;

        Name = name;
        return true;
    }

    public bool SetRole(UserRole role)
    {
        Role = role;
        return true;
    }

    public bool SetPassword(string password)
    {
        Password = password;
        return true;
    }

    public bool AddTrackToFavorites(Track track)
    {
        if (!FavoriteTracks.Any(t => t.Id == track.Id))
        {
            _favoriteTracks.Add(track);
            return true;
        }
        return false;
    }

    public bool RemoveTrackFromFavorites(Track track)
    {
        return _favoriteTracks.RemoveAll(t => t.Id == track.Id) > 0;
    }

    public bool AddAlbumToFavorites(Album album)
    {
        if (!FavoriteAlbums.Any(a => a.Id == album.Id))
        {
            _favoriteAlbums.Add(album);
            return true;
        }
        return false;
    }

    public bool RemoveAlbumFromFavorites(Album album)
    {
        return _favoriteAlbums.RemoveAll(a => a.Id == album.Id) > 0;
    }

    public bool AddArtistToFavorites(Artist artist)
    {
        if (!FavoriteArtists.Any(a => a.Id == artist.Id))
        {
            _favoriteArtists.Add(artist);
            return true;
        }
        return false;
    }

    public bool RemoveArtistFromFavorites(Artist artist)
    {
        return _favoriteArtists.RemoveAll(a => a.Id == artist.Id) > 0;
    }

    public void InitializeTimestamps()
    {
        if (CreatedAt == default)
        {
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
            throw new InvalidOperationException("CreatedAt has already been initialized.");
    }

    public void UpdateUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}