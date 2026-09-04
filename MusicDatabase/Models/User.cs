public class User
{
    public Guid Id {get; set;}
    public string Name {get; set;}
    public UserRole Role {get; set;}
    public string Password {get; set;}
    public List<Track> FavoriteTracks {get; private set;} = null!;
    public List<Album> FavoriteAlbums {get; private set;} = null!;
    public List<Artist> FavoriteArtists {get; private set;} = null!;
    public List<Playlist> Playlists {get; private set;} = null!;

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