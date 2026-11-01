using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class ArtistRepositoryTests(PostgresFixture pg)
{
    [Fact]
    public async Task GetMatchingArtistsAsync_ReturnsCaseInsensitiveMatchesInNameOrder()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var beta = new Artist("Beta Artist");
        var alpha = new Artist("Alpha Artist");
        var other = new Artist("Other Name");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddRangeAsync(beta, alpha, other);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var result = await manager.Artists.GetMatchingArtistsAsync("artist", 0, 10);

            Assert.Equal(["Alpha Artist", "Beta Artist"], result.Items.Select(a => a.Name).ToArray());
        }
    }

    [Fact]
    public async Task GetArtistDetailAsync_LoadsRelatedAlbumsAndTracks()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist One");
        var album = new Album("Album One", 2025, artist);
        var featured = new Artist("Featured Artist");
        var track = new Track("Track One", 1, album, artist, [featured]);
        album.AddTrack(track);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddRangeAsync(artist, featured);
            await db.Albums.AddAsync(album);
            await db.Tracks.AddAsync(track);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var result = await manager.Artists.GetArtistDetailAsync(artist.Id);

            Assert.NotNull(result);
            Assert.Equal("Artist One", result.Name);
            Assert.Single(result.Albums);
            Assert.Equal("Album One", result.Albums.Single().Title);
            Assert.Contains(result.Tracks, t => t.Title == "Track One" && t.Others.Any(o => o.Name == "Featured Artist"));
        }
    }
}
