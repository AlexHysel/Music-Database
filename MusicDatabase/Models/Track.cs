public class Track
{
    public Guid Id {get; set;}
    public string Title {get; set;} = null!;
    public Guid AlbumId {get; set;}
    public Album Album {get; set;} = null!;
    public Guid ArtistId {get; set;}
    public Artist Artist {get; set;} = null!;
    public List<Artist> Others {get; set;} = new List<Artist>();
    public Genre Genre {get; set;}
}