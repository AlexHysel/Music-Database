using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class AlbumRepositoryTests(PostgresFixture pg)
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

    [Fact]
    public async Task CreateAlbumAsync_WithTracks_CreatesArtistsAndTracks()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        
        var artist = new Artist("artist");
        var album = new Album("album", 2025, artist);
        List<Track> tracks = [
            new Track("track1", 1, album, artist, []),
            new Track("track2", 2, album, artist, []),
            new Track("track3", 3, album, artist, [])
        ];

        foreach (var t in tracks) album.AddTrack(t);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await manager.Albums.CreateAlbumAsync(album);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            
            var albumResult = await manager.Albums.GetAlbumDetailAsync(album.Id);

            Assert.NotNull(albumResult);
            Assert.Equal(albumResult.Title, album.Title);
            Assert.Equal(albumResult.ReleaseYear, album.ReleaseYear);

            Assert.Equal(albumResult.Artist.Name, album.Artist.Name);
            Assert.Equal(albumResult.Artist.Id, album.Artist.Id);

            var tracksResultSorted = albumResult.Tracks.OrderBy(t => t.NumberInTheAlbum).ToArray();
            var numberInAlbum = 1;

            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                var trackResult = tracksResultSorted[i];

                Assert.Equal(trackResult.Title, track.Title);
                Assert.Equal(numberInAlbum, trackResult.NumberInTheAlbum);
                numberInAlbum++;
            }
        }
    }

    [Fact]
    public async Task RemoveAlbumAsync_RemovesAlbumAndRelatedEmptyEntities()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        
        var artist = new Artist("artist");
        var album = new Album("album", 2025, artist);
        List<Track> tracks = [
            new Track("track1", 1, album, artist, []),
            new Track("track2", 2, album, artist, []),
            new Track("track3", 3, album, artist, [])
        ];

        var user = new User("user", "password", UserRole.User);
        var tag = new Tag("tag");
        var albumTagStats = new AlbumTagStats(album.Id, tag.Id);
        albumTagStats.AddVote(user);

        foreach (var t in tracks) album.AddTrack(t);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await manager.Albums.CreateAlbumAsync(album);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            
            await manager.Albums.RemoveAlbumAsync(album.Id);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            
            Assert.False(await manager.Albums.AlbumExistsAsync(album.Id));
            Assert.False(await manager.Artists.ArtistExistsAsync(artist.Id));
            foreach (var t in tracks)
            {
                Assert.False(await manager.Tracks.TrackExistsAsync(t.Id));
            }
            Assert.False(await manager.Tags.TagExistsAsync(tag.Id));
            Assert.False(await manager.Tags.AlbumTagStatsExistsAsync(albumTagStats.Id));
        }
    }
}