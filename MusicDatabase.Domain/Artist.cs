namespace MusicDatabase.Domain;

public class Artist
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string ImageUrl { get; private set; } = "";
    private readonly List<Album> _albums = new();
    public IReadOnlyCollection<Album> Albums => _albums.AsReadOnly();
    private readonly List<Track> _tracks = new();
    public IReadOnlyCollection<Track> Tracks => _tracks.AsReadOnly();
    private readonly List<Track> _appearsOn = new();
    public IReadOnlyCollection<Track> AppearsOn => _appearsOn.AsReadOnly();
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public Artist(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Artist name cannot be null or whitespace.", nameof(name));
    
        Id = Guid.NewGuid();
        Name = name;
    }

    private Artist() { }

    public bool SetName(string name)
    {
        name = name.Trim();
        if (name.Length < 1) return false;

        Name = name;
        return true;
    }

    public bool SetImageUrl(string imageUrl)
    {
        imageUrl = imageUrl.Trim();
        if (imageUrl.Length < 1) return false;

        ImageUrl = imageUrl;
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