class Album
{
    public Guid Id {get; set;}
    public string Title {get; set;}
    public Guid ArtistId {get; set;}
    public Artist Artist {get; set;}
    public List<Track> Tracks {get; set;}
    public AlbumType Type {get; set;}
}