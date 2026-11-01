using MusicDatabase.Domain;

namespace MusicDatabase.Tests.UnitTests;

public class AlbumTests
{
    [Fact]
    public void Album_Constructor_ValidatesRequiredValues()
    {
        var artist = new Artist("Artist");

        Assert.Throws<ArgumentException>(() => new Album("   ", 2024, artist));
        Assert.Throws<ArgumentNullException>(() => new Album("Valid title", 2024, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Album("Valid title", 1899, artist));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Album("Valid title", DateTimeOffset.UtcNow.Year + 2, artist));
    }

    [Fact]
    public void Album_InitializeTimestamps_OnlyOnce()
    {
        var artist = new Artist("Artist");
        var album = new Album("Album", 2024, artist);

        album.InitializeTimestamps();

        Assert.NotEqual(default, album.CreatedAt);
        Assert.NotEqual(default, album.UpdatedAt);
        Assert.Throws<InvalidOperationException>(() => album.InitializeTimestamps());
    }

    [Fact]
    public void Album_AddTrack_ReordersNumbersAndKeepsAlbumTrackCollectionConsistent()
    {
        var artist = new Artist("Artist");
        var album = new Album("Album", 2024, artist);

        var trackThree = new Track("Track 3", 3, album, artist, []);
        var trackOne = new Track("Track 1", 1, album, artist, []);

        album.AddTrack(trackThree);
        album.AddTrack(trackOne);

        Assert.Equal([trackOne, trackThree], album.Tracks);
        Assert.Equal(1, album.Tracks[0].NumberInTheAlbum);
        Assert.Equal(2, album.Tracks[1].NumberInTheAlbum);
    }   
}