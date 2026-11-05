namespace MusicDatabase.Domain;

/// <summary>
/// Represents a track.
/// </summary>
/// <remarks>
/// <b>Rules:</b>
/// <list type="bullet">
/// <item><description>Must have an Artist.</description></item>
/// <item><description>Must have an Album.</description></item>
/// <item><description>Others must not contain the main Artist.</description></item>
/// <item><description>Must have a Title (1 to 50 chars).</description></item>
/// <item><description>Track Title must be unique within the album.</description></item>
/// <item><description>TagStats must have no duplicates.</description></item>
/// <item><description>NumberInTheAlbum must be a natural number (>= 1).</description></item>
/// </list>
/// </remarks>
public class Track
{
    public Guid Id {get; private set;}
    public string Title {get; private set;}
    public int NumberInTheAlbum {get; private set;}
    public Guid AlbumId {get; private set;}
    public Album Album {get; private set;}
    public Guid ArtistId {get; private set;}
    public Artist Artist {get; private set;}
    private readonly List<Artist> _others = [];
    public IReadOnlyCollection<Artist> Others => _others.AsReadOnly();
    public readonly List<TrackTagStats> _tagsStats = [];
    public IReadOnlyCollection<TrackTagStats> TagsStats => _tagsStats.AsReadOnly();
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public Track(string title, int numberInTheAlbum, Album album, Artist artist, List<Artist> others)
    {
        title = title.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (title.Length is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(title), "Track title must be between 1 and 50 characters.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberInTheAlbum);
        ArgumentNullException.ThrowIfNull(album);
        ArgumentNullException.ThrowIfNull(artist);

        Id = Guid.NewGuid();
        Title = title;
        NumberInTheAlbum = numberInTheAlbum;
        AlbumId = album.Id;
        Album = album;
        ArtistId = artist.Id;
        Artist = artist;
        _others.AddRange(others);
    }

    private Track () {}

    public void SetTitle(string title)
    {
        title = title.Trim();
        ArgumentException.ThrowIfNullOrEmpty(title);
        if (title.Length is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(title), "Track title must be between 1 and 50 characters.");

        Title = title;
    }

    public static bool ValidateTitle(string title)
    {
        title = title.Trim();
        if (title.Length is < 1 or > 50)
            return false;

        return true;
    }

    public void SetArtist(Artist artist)
    {
        ArgumentNullException.ThrowIfNull(artist);

        Artist = artist;
        ArtistId = artist.Id;
    }

    public void SetOthers(List<Artist> others)
    {
        ArgumentNullException.ThrowIfNull(others, nameof(others));

        if (others.Any(a => a == null))
            throw new ArgumentException("Others cannot contain null values.", nameof(others));

        if (others.Any(a => a.Id == ArtistId))
            throw new ArgumentException("Main artist cannot be featured", nameof(others));

        List<Artist> toDelete = _others.Where(o => !others.Any(other => other.Id == o.Id)).ToList();
        foreach (var del in toDelete)
            _others.Remove(del);
        
        foreach (var o in others)
            if (!Others.Any(other => other.Id == o.Id))
                _others.Add(o);
    }

    public void UpdateUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetNumberInTheAlbum(int number)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number, nameof(number));

        NumberInTheAlbum = number;
        return;
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