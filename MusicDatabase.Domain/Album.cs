namespace MusicDatabase.Domain;

/// <summary>
/// Represents an album.
/// </summary>
/// <remarks>
/// <b>Rules:</b>
/// <list type="bullet">
/// <item><description>
/// Album must have at least 1 track.
/// If it's empty after some chages, SQL triggers will delete it automatically.
/// </description></item>
/// <item><description>Album must have a title.</description></item>
/// <item><description>Album must have an artist.</description></item>
/// <item><description>Album type is defined automatically by SQL triggers depending on the number of tracks.</description></item>
/// <item><description>CreatedAt can be initialized only once.</description></item>
/// <item><description>RealeseYear must be between 1900 and current year + 1.</description></item>
/// </list>
/// </remarks>
public class Album
{
    public Guid Id {get; private set;}
    public string Title { get; private set; } = null!;
    public int ReleaseYear {get; private set;}
    public Guid ArtistId { get; private set; }
    public Artist Artist { get; private set; } = null!;
    public string ImageUrl {get; private set;} = "";
    public List<Track> Tracks {get; private set; } = [];
    public AlbumType Type {get; private set;}
    private readonly List<AlbumTagStats> _tagsStats = [];
    public IReadOnlyCollection<AlbumTagStats> TagsStats => _tagsStats.AsReadOnly();
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

    public void SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);

        Title = title;
    }

    public void SetArtist(Artist artist)
    {
        ArgumentNullException.ThrowIfNull(artist);

        Artist = artist;
        ArtistId = artist.Id;
    }

    public void SetImageUrl(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void SetReleaseYear(int releaseYear)
    {
        if (releaseYear < 1900 || releaseYear > DateTimeOffset.UtcNow.Year + 1)
            throw new ArgumentException($"Invalid ReleaseYear provided: {releaseYear}");

        ReleaseYear = releaseYear;
    }

    public void AddTrack(Track track)
    {
        ArgumentNullException.ThrowIfNull(track, nameof(track));

        if (Tracks.Any(t => t.Id == track.Id))
            throw new ArgumentException($"Track ({track.Id}) already exists in the Album");

        int targetIndex = track.NumberInTheAlbum - 1;
        if (targetIndex >= 0 && targetIndex <= Tracks.Count)
            Tracks.Insert(targetIndex, track);
        else
            Tracks.Add(track);

        for (int i = 0; i < Tracks.Count; i++)
            Tracks[i].SetNumberInTheAlbum(i + 1);
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