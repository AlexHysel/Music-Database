using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class TrackRepositoryTests(PostgresFixture pg)
{
    [Fact]
    public async Task GetMatchingTracksAsync_ReturnsCaseInsensitiveMatches()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var featured = new Artist("Featured");
        var album = new Album("Album", 2025, artist);
        var trackOne = new Track("Alpha Song", 1, album, artist, []);
        var trackTwo = new Track("Beta Song", 2, album, artist, [featured]);
        var other = new Track("Other", 3, album, artist, []);
        album.AddTrack(trackOne);
        album.AddTrack(trackTwo);
        album.AddTrack(other);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddRangeAsync(artist, featured);
            await db.Albums.AddAsync(album);
            await db.Tracks.AddRangeAsync(trackOne, trackTwo, other);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var result = await manager.Tracks.GetMatchingTracksAsync("song", 0, 10);

            Assert.Contains(result.Items, t => t.Title == "Alpha Song");
            Assert.Contains(result.Items, t => t.Title == "Beta Song");
            Assert.DoesNotContain(result.Items, t => t.Title == "Other");
        }
    }

    [Fact]
    public async Task GetTrackDetailAsync_LoadsAlbumArtistAndFeaturedArtists()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var featured = new Artist("Featured");
        var album = new Album("Album", 2025, artist);
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
            var result = await manager.Tracks.GetTrackDetailAsync(track.Id);

            Assert.NotNull(result);
            Assert.Equal("Track One", result!.Title);
            Assert.Equal(album.Id, result.AlbumId);
            Assert.Equal("Artist", result.Artist.Name);
            Assert.Contains(result.Others, a => a.Name == "Featured");
        }
    }
}
