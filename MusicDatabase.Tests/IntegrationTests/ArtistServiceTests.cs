using MusicDatabase.Contracts;
using MusicDatabase.Core.Services;
using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class ArtistServiceTests(PostgresFixture pg)
{
    [Fact]
    public async Task GetMatchingArtistsAsync_ReturnsMatchingArtists()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var alpha = new Artist("Alpha Artist");
        var beta = new Artist("Beta Artist");
        var other = new Artist("Other");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddRangeAsync(alpha, beta, other);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new ArtistService(manager);
            var result = await service.GetMatchingArtistsAsync("artist", 0, 10);

            Assert.Contains(result.Items, a => a.Name == "Alpha Artist");
            Assert.Contains(result.Items, a => a.Name == "Beta Artist");
        }
    }

    [Fact]
    public async Task RemoveArtistAsync_RemovesArtistAndReturnsSuccess()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        var artist = new Artist("Artist");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new ArtistService(manager);
            var result = await service.RemoveArtistAsync(artist.Id);

            Assert.True(result.IsSuccess);
            Assert.False(await manager.Artists.ArtistExistsAsync(artist.Id));
        }
    }

    [Fact]
    public async Task UpdateArtistAsync_UpdatesNameAndImageUrl()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        var artist = new Artist("Artist");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new ArtistService(manager);

            var result = await service.UpdateArtistAsync(new UpdateArtistRequest("Updated Artist", "https://example.com/image.jpg", artist.Id));

            Assert.True(result.IsSuccess);

            var updated = await manager.Artists.GetArtistAsync(artist.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated Artist", updated!.Name);
            Assert.Equal("https://example.com/image.jpg", updated.ImageUrl);
        }
    }

    [Fact]
    public async Task UpdateArtistAsync_MissingArtist_ReturnsNotFound()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new ArtistService(manager);

            var result = await service.UpdateArtistAsync(new UpdateArtistRequest("Updated Artist", "https://example.com/image.jpg", Guid.NewGuid()));

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        }
    }

    [Fact]
    public async Task GetArtistDetailAsync_ReturnsAlbumAndTrackInfo()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var album = new Album("Album One", 2025, artist);
        var track = new Track("Track One", 1, album, artist, []);
        album.AddTrack(track);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await db.Albums.AddAsync(album);
            await db.Tracks.AddAsync(track);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new ArtistService(manager);
            var result = await service.GetArtistDetailAsync(artist.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal("Artist", result.Value.Name);
            Assert.Contains(result.Value.Albums, a => a.Title == "Album One");
            Assert.Contains(result.Value.Tracks, t => t.Title == "Track One");
        }
    }
}
