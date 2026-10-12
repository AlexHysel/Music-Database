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

abstract public class EntityTag
{
    public Tag Tag {get; private set;}
    public Guid Id {get; private set;}
    public User TaggedBy {get; private set;}
    public Guid TaggedById {get; private set;}

    protected EntityTag() {}

    protected EntityTag(Guid userId, Guid tagId)
    {
        TaggedById = userId;
        Id = tagId;
    }
}

public class TrackTag : EntityTag
{
    public Track Track {get; private set;}
    public Guid TrackId {get; private set;}

    private TrackTag() {}

    public TrackTag(Guid userId, Guid tagId, Guid trackId) : base(userId, tagId)
    {
        TrackId = trackId;
    }
}