
using MusicDatabase.Core.Services;
using MusicDatabase.Data;
using MusicDatabase.Contracts;

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
}