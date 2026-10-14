using MusicDatabase.Domain;

public class Tag
{
    public Guid Id {get; private set;}
    public string Name {get; private set;}

    private Tag() {}

    public Tag(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        name = name.Trim();
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (name.Length > 20) throw new ArgumentException("Name of the tag is > 20 chars.");

        Id = Guid.NewGuid();
        Name = name.ToLowerInvariant();
    }
}

public class TrackTagVote
{
    public Guid Id {get; private set;}
    public Track Track {get; private set;}
    public Guid TrackId {get; private set;}
    public Tag Tag {get; private set;}
    public Guid TagId {get; private set;}
    public User TaggedBy {get; private set;}
    public Guid TaggedById {get; private set;}

    public TrackTagVote(Guid trackId, Guid tagId, Guid taggedById)
    {
        TrackId = trackId;
        TagId = tagId;
        TaggedById = taggedById;
    }
}

public class AlbumTagVote
{
    public Guid Id {get; private set;}
    public Album Album {get; private set;}
    public Guid AlbumId {get; private set;}
    public Tag Tag {get; private set;}
    public Guid TagId {get; private set;}
    public User TaggedBy {get; private set;}
    public Guid TaggedById {get; private set;}

    public AlbumTagVote(Guid albumId, Guid tagId, Guid taggedById)
    {
        AlbumId = albumId;
        TagId = tagId;
        TaggedById = taggedById;
    }
}