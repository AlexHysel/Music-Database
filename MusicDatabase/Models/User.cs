public class User
{
    public Guid Id {get; set;}
    public string Name {get; set;}
    public UserRole Role {get; set;}
    public string Password {get; set;}
    public List<Track> FavoriteTracks {get; set;} = new List<Track>();
    public List<Album> FavoriteAlbums {get; set;} = new List<Album>();
    public List<Artist> FavoriteArtists {get; set;} = new List<Artist>();
    public List<Playlist> Playlists {get; set;} = new List<Playlist>();
}