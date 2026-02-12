class Album
{
    public Guid Id {get; set;}
    public string Name {get; set;}
    public List<Artist> Artists {get; set;}
    public List<Track> Tracks {get; set;}
    public AlbumType Type {get; set;}
}