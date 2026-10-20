using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.UnitTests;

[Collection("postgres")]
public class MusicManagerTests(PostgresFixture pg)
{
    [Fact]
    public async Task GetArtistTracksAsync_FiltersByArtistAndKeepsStableOrder()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artistOne = new Artist("Artist One");
        var artistTwo = new Artist("Artist Two");
        
        var albumOne = new Album("Album One", 2000, artistOne);
        var albumTwo = new Album("Album Two", 2001, artistTwo);

        await using (var db = newDb())
        {
            await db.Artists.AddRangeAsync(artistOne, artistTwo);
            await db.Albums.AddRangeAsync(albumOne, albumTwo);

            await db.Tracks.AddRangeAsync(
                new Track("zebra", 1, albumOne, artistOne, []),
                new Track("alpha", 2, albumOne, artistOne, []),
                new Track("beta", 3, albumTwo, artistTwo, [])
            );

            await db.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var result = await manager.Tracks.GetArtistTracksAsync(artistOne.Id, 0, 10);

            Assert.Equal(new[] { "alpha", "zebra" }, result.Items.Select(t => t.Title).ToArray());
            Assert.Equal(2, result.Items.Count);
            Assert.True(result.Items.All(t => t.ArtistId == artistOne.Id));
        }
    }
}