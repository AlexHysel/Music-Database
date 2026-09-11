public class User(string name, string password, UserRole role)
{
    public Guid Id {get; private set;} = Guid.NewGuid();
    public string Name {get; private set;} = name;
    public UserRole Role {get; private set;} = role;
    public string Password {get; private set;} = password;
    public List<Track> FavoriteTracks {get; private set;} = null!;
    public List<Album> FavoriteAlbums {get; private set;} = null!;
    public List<Artist> FavoriteArtists {get; private set;} = null!;
    public List<Playlist> Playlists {get; private set;} = null!;

    public bool SetName(string name)
    {
        if (name.Trim().Length < 1) return false;
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
        if (password.Trim().Length < 1) return false;
        Password = password;
        return true;
    }

    public bool AddTrackToFavorites(Track track)
    {
        if (!FavoriteTracks.Any(t => t.Id == track.Id))
        {
            FavoriteTracks.Add(track);
            return true;
        }
        return false;
    }

    public bool RemoveTrackFromFavorites(Track track)
    {
        if (FavoriteTracks.Any(t => t.Id == track.Id))
        {
            FavoriteTracks.Remove(track);
            return true;
        }
        return false;
    }

    public List<Track> GetFavoriteTracks()
    {
        return FavoriteTracks;
    }

    public bool AddAlbumToFavorites(Album album)
    {
        if (!FavoriteAlbums.Any(a => a.Id == album.Id))
        {
            FavoriteAlbums.Add(album);
            return true;
        }
        return false;
    }

    public bool RemoveAlbumFromFavorites(Album album)
    {
        if (FavoriteAlbums.Any(a => a.Id == album.Id))
        {
            FavoriteAlbums.Remove(album);
            return true;
        }
        return false;
    }

    public List<Album> GetFavoriteAlbums()
    {
        return FavoriteAlbums;
    }

    public bool AddArtistToFavorites(Artist artist)
    {
        if (!FavoriteArtists.Any(a => a.Id == artist.Id))
        {
            FavoriteArtists.Add(artist);
            return true;
        }
        return false;
    }

    public bool RemoveArtistFromFavorites(Artist artist)
    {
        if (FavoriteArtists.Any(a => a.Id == artist.Id))
        {
            FavoriteArtists.Remove(artist);
            return true;
        }
        return false;
    }

    public List<Artist> GetFavoriteArtists()
    {
        return FavoriteArtists;
    }
}