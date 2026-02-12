class Artist
{
    public Guid Id {get; set;}
    public string Name {get; set;}
    public List<Album> Albums {get; set;}
    public List<Track> Tracks {get; set;}
}