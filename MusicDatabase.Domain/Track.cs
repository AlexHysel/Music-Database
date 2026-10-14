namespace MusicDatabase.Domain;

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
    public readonly List<TrackTagVote> _tags = [];
    public IReadOnlyCollection<TrackTagVote> Tags => _tags.AsReadOnly();
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public Track(string title, int numberInTheAlbum, Album album, Artist artist, List<Artist> others)
    {
        title = title.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegative(numberInTheAlbum);
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

    public bool SetTitle(string title)
    {
        title = title.Trim();
        if (title.Length < 1) return false;

        Title = title;
        return true;
    }

    public bool AddTag(Tag tag, User user)
    {
        if (_tags.Any(t => t.TaggedById == user.Id && t.Tag.Id == tag.Id))
            return false;

        TrackTagVote trackTag = new(user.Id, tag.Id, Id);
        _tags.Add(trackTag);
        return true;
    }

    public bool SetOthers(List<Artist> others)
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
        return true;
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