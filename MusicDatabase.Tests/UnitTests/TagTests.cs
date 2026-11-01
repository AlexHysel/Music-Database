using MusicDatabase.Domain;

namespace MusicDatabase.Tests;

public class TagTests
{
    [Fact]
    public void Tag_Constructor_ValidatesNameAndNormalizesCase()
    {
        Assert.Throws<ArgumentException>(() => new Tag("   "));
        Assert.Throws<ArgumentException>(() => new Tag(new string('t', 21)));

        var tag = new Tag("  TagName  ");

        Assert.Equal("tagname", tag.Name);
    }

    [Fact]
    public void AlbumTagStats_AddVote_RejectsDuplicateUsersAndCountsVotesOnce()
    {
        var user = new User("user", "password");
        var tag = new Tag("tag");
        var album = new Album("Album", 2024, new Artist("Artist"));
        var stats = new AlbumTagStats(album.Id, tag.Id);

        Assert.True(stats.AddVote(user));
        Assert.False(stats.AddVote(user));
        Assert.Equal(1, stats.VoteCount);
        Assert.Single(stats.VotedBy);
    }

    [Fact]
    public void TrackTagStats_AddVote_RejectsDuplicateUsersAndCountsVotesOnce()
    {
        var user = new User("user", "password");
        var tag = new Tag("tag");
        var artist = new Artist("Artist");
        var album = new Album("Album", 2024, artist);
        var track = new Track("Song", 1, album, artist, []);
        var stats = new TrackTagStats(track.Id, tag.Id);

        Assert.True(stats.AddVote(user));
        Assert.False(stats.AddVote(user));
        Assert.Equal(1, stats.VoteCount);
        Assert.Single(stats.VotedBy);
    }
}
