using Microsoft.Extensions.Configuration;
using MusicDatabase.Contracts;
using MusicDatabase.Core.Services;
using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class TrackServiceTests(PostgresFixture pg)
{
    [Fact]
    public async Task UpdateTrackAsync_UpdatesTitleAndFeaturedArtists()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var album = new Album("Album", 2025, artist);
        var track = new Track("Old Title", 1, album, artist, []);
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
            var service = new TrackService(manager);

            var result = await service.UpdateTrackAsync(new UpdateTrackRequest("New Title", ["Guest Artist"], track.Id));
            Assert.True(result.IsSuccess);

            var updated = await manager.Tracks.GetTrackDetailAsync(track.Id);
            Assert.NotNull(updated);
            Assert.Equal("New Title", updated!.Title);
            Assert.Contains(updated.Others, a => a.Name == "Guest Artist");
        }
    }

    [Fact]
    public async Task AddTagOnTrackAsync_DuplicateTag_ReturnsConflict()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var album = new Album("Album", 2025, artist);
        var track = new Track("Track One", 1, album, artist, []);
        album.AddTrack(track);
        var user = new User("user", "password");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await db.Albums.AddAsync(album);
            await db.Users.AddAsync(user);
            await db.Tracks.AddAsync(track);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new TrackService(manager);

            var first = await service.AddTagOnTrackAsync(new AddTagOnTrackRequest("tag", track.Id, user.Id));
            var second = await service.AddTagOnTrackAsync(new AddTagOnTrackRequest("tag", track.Id, user.Id));

            Assert.True(first.IsSuccess);
            Assert.False(second.IsSuccess);
            Assert.Equal(ErrorType.Conflict, second.Error!.Type);
        }
    }
}
