using MusicDatabase.Domain;

namespace MusicDatabase.Tests.UnitTests;

public class TrackTests
{
    [Fact]
    public void Track_Constructor_AndSetNumberInTheAlbum_ValidateRequiredValues()
    {
        var artist = new Artist("Artist");
        var album = new Album("Album", 2024, artist);

        Assert.Throws<ArgumentException>(() => new Track("   ", 1, album, artist, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Track("Song", 0, album, artist, []));
        Assert.Throws<ArgumentNullException>(() => new Track("Song", 1, null!, artist, []));
        Assert.Throws<ArgumentNullException>(() => new Track("Song", 1, album, null!, []));

        var track = new Track("Song", 1, album, artist, []);

        Assert.Throws<ArgumentOutOfRangeException>(() => track.SetNumberInTheAlbum(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => track.SetNumberInTheAlbum(-1));
    }

    [Fact]
    public void Track_SetOthers_RejectsMainArtistAndKeepsDistinctFeatureList()
    {
        var artist = new Artist("Artist");
        var album = new Album("Album", 2024, artist);
        var featureOne = new Artist("Feature One");
        var featureTwo = new Artist("Feature Two");
        var track = new Track("Song", 1, album, artist, [featureOne]);

        Assert.Throws<ArgumentException>(() => track.SetOthers([artist]));

        track.SetOthers([featureOne, featureTwo]);

        Assert.Equal(2, track.Others.Count);
        Assert.Contains(featureOne, track.Others);
        Assert.Contains(featureTwo, track.Others);
    }

    [Fact]
    public void Track_InitializeTimestamps_OnlyOnce()
    {
        var artist = new Artist("Artist");
        var album = new Album("Album", 2024, artist);
        var track = new Track("Song", 1, album, artist, []);

        track.InitializeTimestamps();

        Assert.NotEqual(default, track.CreatedAt);
        Assert.NotEqual(default, track.UpdatedAt);
        Assert.Throws<InvalidOperationException>(() => track.InitializeTimestamps());
    }
}