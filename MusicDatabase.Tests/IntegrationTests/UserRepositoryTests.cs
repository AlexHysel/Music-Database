using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class UserRepositoryTests(PostgresFixture pg)
{
    [Fact]
    public async Task AuthenticateUser_ReturnsUser()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var password = "password";
        var username = "user";

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            await manager.Users.CreateUserAsync(username, UserRole.User, password);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var result = await manager.Users.AuthenticateUser(username, password);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateUsername_UserNotCreated()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var password = "password";
        var username = "user";

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var created = await manager.Users.CreateUserAsync(username, UserRole.User, password);
            Assert.True(created);

            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var created = await manager.Users.CreateUserAsync(username, UserRole.User, password);
            Assert.False(created);

            await manager.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task RemoveUserAsync_HasTags_TagsRemoved()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var password = "password";
        var username = "user";
        var artist = new Artist("artist"); 
        var album = new Album("album", 2025, artist);
        var track = new Track("track", 1, album, artist, []);
        album.AddTrack(track);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var created = await manager.Users.CreateUserAsync(username, UserRole.User, password);
            Assert.True(created);

            await manager.Albums.CreateAlbumAsync(album);

            await manager.SaveChangesAsync();
        }

        Guid tagId;

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var user = await manager.Users.GetUserAsync("user", true);
            Assert.NotNull(user);

            Tag tag = await manager.Tags.EnsureTagCreatedAsync("tag");
            Assert.NotNull(tag);
            tagId = tag.Id;

            Assert.True(await manager.Tags.VoteForAlbumTagAsync(album.Id, tag, user));
            Assert.True(await manager.Tags.VoteForTrackTagAsync(track.Id, tag, user));

            await manager.SaveChangesAsync();
        }

        Guid albumTagId;
        Guid trackTagId;

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var user = await manager.Users.GetUserAsync(username);
            Assert.NotNull(user);
            albumTagId = (await manager.Tags.GetAlbumTagStatsAsync(album.Id, 1))[0].Id;
            trackTagId = (await manager.Tags.GetTrackTagStatsAsync(track.Id, 1))[0].Id;

            var result = await manager.Users.RemoveUserAsync(user.Id);
            Assert.True(result);
            await manager.SaveChangesAsync();
        }
        
        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            Assert.False(await manager.Tags.AlbumTagStatsExistsAsync(albumTagId));
            Assert.False(await manager.Tags.TrackTagStatsExistsAsync(trackTagId));
            Assert.False(await manager.Tags.TagExistsAsync(tagId));
        }
    }
}