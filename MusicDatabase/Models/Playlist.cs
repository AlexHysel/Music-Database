class Playlist
{
    public Guid Id {get; set;}
    public string Title {get; set;}
    public Guid CreatorId {get; set;}
    public User Creator {get; set;}
    public List<Track> Tracks {get; set;}
}