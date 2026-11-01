using MusicDatabase.Domain;

namespace MusicDatabase.Tests.UnitTests;

public class ArtistTests
{
    [Fact]
    public void Artist_Constructor_AndSetName_ValidateNameRules()
    {
        Assert.Throws<ArgumentException>(() => new Artist("   "));
        Assert.True(Artist.ValidateName(" Artist "));
        Assert.False(Artist.ValidateName("   "));
        Assert.False(Artist.ValidateName(null));

        var artist = new Artist("Artist");
        artist.SetName("  Updated Artist  ");

        Assert.Equal("Updated Artist", artist.Name);
    }

    [Fact]
    public void Artist_InitializeTimestamps_OnlyOnce()
    {
        var artist = new Artist("Artist");

        artist.InitializeTimestamps();

        Assert.NotEqual(default, artist.CreatedAt);
        Assert.NotEqual(default, artist.UpdatedAt);
        Assert.Throws<InvalidOperationException>(() => artist.InitializeTimestamps());
    }
}