namespace MusicDatabase.Data;

public class Artist
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string ImageUrl { get; private set; } = "";
    public List<Album> Albums { get; private set; } = new();
    public List<Track> Tracks { get; private set; } = new();
    public List<Track> AppearsOn {get; private set;} = new();
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public Artist(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    private Artist() { }

    public bool SetName(string name)
    {
        if (name.Trim().Length < 1) return false;
        Name = name;
        return true;
    }

    public bool SetImageUrl(string imageUrl)
    {
        ImageUrl = imageUrl;
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