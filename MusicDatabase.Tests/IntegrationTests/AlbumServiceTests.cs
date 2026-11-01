
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
            Assert.False(result.IsSuccess);
        }
    }

    [Fact]
    public async Task CreateAlbumAsync_Duplicate_AlbumNotCreated()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        
        var album = new AddAlbumRequest("album", "artist", "imageUrl", 2025, [
            new AddTrackRequest("track", [])
        ]);

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);
            var result = await albumService.CreateAlbumAsync(album);
            Assert.True(result.IsSuccess);
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);
            var result = await albumService.CreateAlbumAsync(album);
            Assert.False(result.IsSuccess);
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

            Assert.True((await albumService.CreateAlbumAsync(album)).IsSuccess);
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
            Assert.True((await albumService.UpdateAlbumAsync(request)).IsSuccess);
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            Assert.False(await manager.Artists.ArtistExistsAsync("other1"));
            Assert.True(await manager.Artists.ArtistExistsAsync("other2"));
        }
    }
    
    [Fact]
    public async Task UpdateAlbumAsync_ChangeArtist_EmptyArtistRemoved()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        AddTrackRequest[] tracks = [
                new AddTrackRequest("track1", [])
            ];

        var album = new AddAlbumRequest("album", "artist", "imageUrl", 2025, tracks);
        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);

            Assert.True((await albumService.CreateAlbumAsync(album)).IsSuccess);
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);

            var trackId = (await manager.Tracks.GetMatchingTracksAsync("track1", 0, 1)).Items[0].Id;
            var albumId = (await manager.Albums.GetMatchingAlbumsAsync("album", 0, 1)).Items[0].Id;

            UpdateTrackInAlbumRequest[] trackRequests = [
                new UpdateTrackInAlbumRequest("track1", "artistNew", [], 2026, trackId)
            ];

            var request = new UpdateAlbumRequest("album", "artistNew", "imageUrl", 2025, albumId, trackRequests);
            Assert.True((await albumService.UpdateAlbumAsync(request)).IsSuccess);
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            Assert.False(await manager.Artists.ArtistExistsAsync("artist"));
            Assert.True(await manager.Artists.ArtistExistsAsync("artistNew"));
        }
    }

    [Fact]
    public async Task AddTagOnAlbumAsync_DuplicateTag_ReturnsConflict()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("artist");
        var album = new Album("album", 2025, artist);
        var track = new Track("track1", 1, album, artist, []);
        album.AddTrack(track);
        var user = new User("user", "password");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await db.Users.AddAsync(user);
            await db.Albums.AddAsync(album);
            await db.Tracks.AddAsync(track);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);

            var first = await albumService.AddTagOnAlbumAsync(new AddTagOnAlbumRequest("tag", album.Id, user.Id));
            var second = await albumService.AddTagOnAlbumAsync(new AddTagOnAlbumRequest("tag", album.Id, user.Id));

            Assert.True(first.IsSuccess);
            Assert.False(second.IsSuccess);
            Assert.Equal(ErrorType.Conflict, second.Error!.Type);
        }
    }

    [Fact]
    public async Task GetAlbumDetailAsync_ExistingAlbum_ReturnsDetailDto()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("artist");
        var album = new Album("album", 2025, artist);
        var track = new Track("track1", 1, album, artist, []);
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
            var albumService = new AlbumService(manager);

            var result = await albumService.GetAlbumDetailAsync(album.Id);
            Assert.True(result.IsSuccess);
            Assert.Equal("album", result.Value.Title);
            Assert.Contains(result.Value.Tracks, t => t.Title == "track1");
        }
    }

    [Fact]
    public async Task RemoveTagOnAlbumAsync_RemovesUserTagVote()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var artist = new Artist("artist");
        var album = new Album("album", 2025, artist);
        var track = new Track("track1", 1, album, artist, []);
        album.AddTrack(track);
        var user = new User("user", "password");

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            await db.Artists.AddAsync(artist);
            await db.Albums.AddAsync(album);
            await db.Tracks.AddAsync(track);
            await db.Users.AddAsync(user);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var albumService = new AlbumService(manager);

            var add = await albumService.AddTagOnAlbumAsync(new AddTagOnAlbumRequest("tag", album.Id, user.Id));
            Assert.True(add.IsSuccess);

            var remove = await albumService.RemoveTagOnAlbumAsync(new RemoveTagOnAlbumRequest("tag", album.Id, user.Id));
            Assert.True(remove.IsSuccess);

            var detail = await albumService.GetAlbumDetailAsync(album.Id);
            Assert.True(detail.IsSuccess);
            Assert.DoesNotContain(detail.Value.Tags, tag => tag.Name == "tag");
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

        var user = new User("user", "password");
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
            var albumService = new AlbumService(manager);
            
            Assert.True((await albumService.RemoveAlbumAsync(album.Id)).IsSuccess);
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