using Microsoft.EntityFrameworkCore;
using MusicDatabase.Data;

namespace MusicDatabase.Tests;

public class MusicManagerTests
{
    [Fact]
    public async Task GetArtistTracksAsync_FiltersByArtistAndKeepsStableOrder()
    {
        var options = new DbContextOptionsBuilder<MusicDb>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new MusicDb(options);

        var artistOne = new Artist("Artist One");
        var artistTwo = new Artist("Artist Two");
        await db.Artists.AddRangeAsync(artistOne, artistTwo);

        var albumOne = new Album("Album One", artistOne);
        var albumTwo = new Album("Album Two", artistTwo);
        await db.Albums.AddRangeAsync(albumOne, albumTwo);

        await db.Tracks.AddRangeAsync(
            new Track("zebra", albumOne, artistOne, new List<Artist>(), Genre.ROCK),
            new Track("alpha", albumOne, artistOne, new List<Artist>(), Genre.ROCK),
            new Track("beta", albumTwo, artistTwo, new List<Artist>(), Genre.ROCK)
        );

        await db.SaveChangesAsync();

        var manager = new MusicManager(db);
        var result = await manager.GetArtistTracksAsync(artistOne.Id, 0, 10);

        Assert.Equal(new[] { "alpha", "zebra" }, result.Items.Select(t => t.Title).ToArray());
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.Items.All(t => t.ArtistId == artistOne.Id));
    }
}