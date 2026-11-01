using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class TagRepositoryTests(PostgresFixture pg)
{
    [Fact]
    public async Task VoteForTrackTagAsync_RejectsDuplicateVotesFromSameUser()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var album = new Album("Album", 2025, artist);
        var track = new Track("Track", 1, album, artist, []);
        album.AddTrack(track);
        var user = new User("user", "password");
        var tag = new Tag("tag");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await db.Users.AddAsync(user);
            await db.Tags.AddAsync(tag);
            await db.Albums.AddAsync(album);
            await db.Tracks.AddAsync(track);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var storedUser = await manager.Users.GetUserAsync("user", true);
            var storedTag = await manager.Tags.EnsureTagCreatedAsync("tag");

            Assert.NotNull(storedUser);
            Assert.True(await manager.Tags.VoteForTrackTagAsync(track.Id, storedTag, storedUser!));
            Assert.False(await manager.Tags.VoteForTrackTagAsync(track.Id, storedTag, storedUser!));

            await manager.SaveChangesAsync();

            var stats = await manager.Tags.GetTrackTagStatsAsync(track.Id, 10);
            Assert.Single(stats);
            Assert.Equal(1, stats[0].VoteCount);
        }
    }

    [Fact]
    public async Task VoteForAlbumTagAsync_RejectsDuplicateVotesFromSameUser()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("Artist");
        var album = new Album("Album", 2025, artist);
        var user = new User("user", "password");
        var tag = new Tag("tag");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await db.Users.AddAsync(user);
            await db.Tags.AddAsync(tag);
            await db.Albums.AddAsync(album);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var storedUser = await manager.Users.GetUserAsync("user", true);
            var storedTag = await manager.Tags.EnsureTagCreatedAsync("tag");

            Assert.NotNull(storedUser);
            Assert.True(await manager.Tags.VoteForAlbumTagAsync(album.Id, storedTag, storedUser!));
            Assert.False(await manager.Tags.VoteForAlbumTagAsync(album.Id, storedTag, storedUser!));

            await manager.SaveChangesAsync();

            var stats = await manager.Tags.GetAlbumTagStatsAsync(album.Id, 10);
            Assert.Single(stats);
            Assert.Equal(1, stats[0].VoteCount);
        }
    }
}
