using MusicDatabase.Domain;

// Represents a Tag without any specifics and used for both albums and tracks
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

public class TrackTagStats
{
    public Guid Id {get; private set;} = Guid.NewGuid();
    public Track Track {get; private set;}
    public Guid TrackId {get; private set;}
    public Tag Tag {get; private set;}
    public Guid TagId {get; private set;}
    public IReadOnlyCollection<User> VotedBy => _votedBy.AsReadOnly();
    private readonly List<User> _votedBy = [];
    public int VoteCount {get; private set;} = 0;

    public TrackTagStats(Guid trackId, Guid tagId)
    {
        TrackId = trackId;
        TagId = tagId;
    }

    public bool AddVote(User user)
    {
        if (VotedBy.Any(u => u.Id == user.Id)) return false;

        _votedBy.Add(user);
        VoteCount++;
        return true;
    }
}

public class AlbumTagStats
{
    public Guid Id {get; private set;} = Guid.NewGuid();
    public Album Album {get; private set;}
    public Guid AlbumId {get; private set;}
    public Tag Tag {get; private set;}
    public Guid TagId {get; private set;}
    public IReadOnlyCollection<User> VotedBy => _votedBy.AsReadOnly();
    private readonly List<User> _votedBy = [];
    public int VoteCount {get; private set;} = 0;

    public AlbumTagStats(Guid albumId, Guid tagId)
    {
        AlbumId = albumId;
        TagId = tagId;
    }

    public bool AddVote(User user)
    {
        if (VotedBy.Any(u => u.Id == user.Id)) return false;

        _votedBy.Add(user);
        VoteCount++;
        return true;
    }
}