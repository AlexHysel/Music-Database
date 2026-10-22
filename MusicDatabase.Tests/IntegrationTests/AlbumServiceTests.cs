
using MusicDatabase.Core.Services;
using MusicDatabase.Data;
using MusicDatabase.Contracts;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class AlbumServiceTests(PostgresFixture pg)
{
    [Fact]
    public async Task CreateAlbumAsync_NoTracks_AlbumNotCreated()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        
        var album = new AddAlbumRequest("album", "artist", "imageUrl", 2025, []);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);
            var result = await albumService.CreateAlbumAsync(album);
            Assert.False(result.Success);
        }
    }

    [Fact]
    public async Task UpdateAlbumAsync_ChangeOthersOnTrack_EmptyOtherDeleted()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        AddTrackRequest[] tracks = [
                new AddTrackRequest("track1", ["other1"])
            ];

        var album = new AddAlbumRequest("album", "artist", "imageUrl", 2025, tracks);
        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);

            Assert.True((await albumService.CreateAlbumAsync(album)).Success);
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);

            var trackId = (await manager.Tracks.GetMatchingTracksAsync("track1", 0, 1)).Items[0].Id;
            var albumId = (await manager.Albums.GetMatchingAlbumsAsync("album", 0, 1)).Items[0].Id;

            UpdateTrackInAlbumRequest[] trackRequests = [
                new UpdateTrackInAlbumRequest("track1", "artist", ["other2"], 1, trackId)
            ];

            var request = new UpdateAlbumRequest("album", "artist", "imageUrl", 2025, albumId, trackRequests);
            Assert.True((await albumService.UpdateAlbumAsync(request)).Success);
        }
        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            Assert.False(await manager.Artists.ArtistExistsAsync("other1"));
            Assert.True(await manager.Artists.ArtistExistsAsync("other2"));
        }
    }
}