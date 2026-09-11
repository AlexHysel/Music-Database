public class Album
{
    public Guid Id {get; private set;}
    public string Title { get; private set; }
    public Guid ArtistId { get; private set; }
    public Artist Artist { get; private set; }
    public string ImageUrl {get; private set;} = "";
    public List<Track> Tracks {get; private set; } = new List<Track>();
    public AlbumType Type {get; private set;}

    public Album (string title, Artist artist)
    {
        Id = Guid.NewGuid();
        Title = title;
        ArtistId = artist.Id;
        Artist = artist;
    }

    private Album () {}

    public bool SetTitle(string title)
    {
        if (title.Trim().Length < 1) return false;
        Title = title;
        return true;
    }

    public bool SetArtist(Artist artist)
    {
        Artist = artist;
        ArtistId = artist.Id;
        return true;
    }

    public bool SetImageUrl(string imageUrl)
    {
        if (imageUrl.Trim().Length < 1) return false;
        ImageUrl = imageUrl;
        return true;
    }

    public bool SetTracks(List<Track> tracks)
    {
        Tracks = tracks;
        return true;
    }
}