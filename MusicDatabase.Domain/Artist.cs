namespace MusicDatabase.Domain;

/// <summary>
/// Represents an artist.
/// </summary>
/// <remarks>
/// <b>Rules:</b>
/// <list type="bullet">
/// <item><description>
/// Artist must have at least 1 album or 1 appearsOn. 
/// If it's empty after some chages, SQL triggers will delete it automatically.
/// </description></item>
/// <item><description>Artist must have a unique name.</description></item>
/// <item><description>Artist can not have a few albums with the same names.</description></item>
/// <item><description>CreatedAt can be initialized only once.</description></item>
/// </list>
/// </remarks>
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