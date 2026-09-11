public class Artist
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string ImageUrl { get; private set; } = "";
    public List<Album> Albums { get; private set; } = new();
    public List<Track> Tracks { get; private set; } = new();

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
}