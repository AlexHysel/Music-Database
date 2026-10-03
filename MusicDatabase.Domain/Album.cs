namespace MusicDatabase.Domain;

public class Album
{
    public Guid Id {get; private set;}
    public string Title { get; private set; } = null!;
    public int ReleaseYear {get; private set;}
    public Guid ArtistId { get; private set; }
    public Artist Artist { get; private set; } = null!;
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
        ArgumentNullException.ThrowIfNull(artist);
        //what if new artist is one of the 'Others' in the track?

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

    public bool SetReleaseYear(int releaseYear)
    {
        if (releaseYear < 1900 || releaseYear > DateTimeOffset.UtcNow.Year + 1)
            return false;

        ReleaseYear = releaseYear;
        return true;
    }

    public bool SetTracks(List<Track> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks, nameof(tracks));
        if (tracks.Any(t => t == null))
            throw new ArgumentException("Tracks cannot be null.", nameof(tracks));

        tracks.OrderBy(t => t.NumberInTheAlbum);
        Tracks.Clear();
        for (int i = 1; i <= tracks.Count; i++)
        {
            var track = tracks[i - 1];
            track.SetNumberInTheAlbum(i);
            Tracks.Add(track);
        }
        return true;
    }

    public void UpdateUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void InitializeTimestamps()
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