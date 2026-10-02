namespace MusicDatabase.Data;

public class Album
{
    public Guid Id {get; private set;}
    public string Title { get; private set; }
    public int ReleaseYear {get; private set;}
    public Guid ArtistId { get; private set; }
    public Artist Artist { get; private set; }
    public string ImageUrl {get; private set;} = "";
    public List<Track> Tracks {get; private set; } = new List<Track>();
    public AlbumType Type {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public Album (string title, int releaseYear, Artist artist, string imageUrl = "")
    {
        title = title.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(releaseYear);
        ArgumentNullException.ThrowIfNull(artist);
        if (releaseYear < 1900 || releaseYear > DateTimeOffset.UtcNow.Year + 1)
            throw new ArgumentOutOfRangeException(nameof(releaseYear), $"Release year must be between 1900 and {DateTimeOffset.UtcNow.Year + 1}.");

        Id = Guid.NewGuid();
        Title = title;
        ReleaseYear = releaseYear;
        ArtistId = artist.Id;
        Artist = artist;
        ImageUrl = imageUrl;
    }

    private Album () {}

    public bool SetTitle(string title)
    {
        title = title.Trim();
        if (title.Length < 1) return false;

        Title = title;
        return true;
    }

    public bool SetArtist(Artist artist)
    {
        if (artist == null) return false;

        Artist = artist;
        ArtistId = artist.Id;
        return true;
    }

    public bool SetImageUrl(string imageUrl)
    {
        imageUrl = imageUrl.Trim();
        if (imageUrl.Length < 1) return false;

        ImageUrl = imageUrl;
        return true;
    }

    public bool SetTracks(List<Track> tracks)
    {
        Tracks = tracks;
        return true;
    }

    internal void UpdateUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void InitializeTimestamps()
    {
        if (CreatedAt == default)
        {
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
            throw new InvalidOperationException("CreatedAt has already been initialized.");
    }
}