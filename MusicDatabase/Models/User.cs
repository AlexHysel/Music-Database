class User
{
    public Guid Id {get; set;}
    public string Name {get; set;}
    public string Password {get; set;}
    public List<Track> FavoriteTracks {get; set;}
    public List<Album> FavoriteAlbums {get; set;}
    public List<Artist> FavoriteArtists {get; set;}
    public List<Playlist> Playlists {get; set;}
}