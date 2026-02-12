class Track
{
    public Guid Id {get; set;}
    public string Title {get; set;}
    public Guid AlbumId {get; set;}
    public Album Album {get; set;}
    public List<Artist> Artists {get; set;}
    public Genre Genre {get; set;}
}