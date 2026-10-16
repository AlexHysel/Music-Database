using MusicDatabase.Data;
using MusicDatabase.Contracts;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Core.Services;

public class AlbumService
{
    private readonly IMusicManager _manager;

    public AlbumService(MusicManager manager)
    {
        _manager = manager;
    }

    public async Task CreateAlbumAsync(AddAlbumRequest info)
    {
        Artist artist = await _manager.Artists.EnsureArtistCreated(info.ArtistName);
        Album album = new(info.Title, info.ReleaseYear, artist, info.ImageUrl);

        int n = 1;
        foreach (AddTrackRequest trackInfo in info.Tracks)
        {
            List<Artist> others = new();
            if (trackInfo.Others != null)
                foreach (string name in trackInfo.Others)
                    if (!string.IsNullOrEmpty(name))
                        others.Add(await _manager.Artists.EnsureArtistCreated(name));
            Track track = new(trackInfo.Title, n++, album, artist, others);
            album.Tracks.Add(track);
        }

        await _manager.Albums.CreateAlbumAsync(album);
        await _manager.SaveChangesAsync();
    }

    public async Task<Result> AddTagOnAlbumAsync(AddTagOnAlbumRequest request)
    {
        Tag tag = await _manager.Tags.EnsureTagCreated(request.TagName);
        AlbumTagVote trackTag = new(request.AlbumId, tag.Id, request.UserId);
        if (await _manager.Albums.AddTagOnAlbumAsync(trackTag))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("You've already added this tag.");
    }

    public async Task<Result<AlbumDTO[]>> GetFavoriteAlbumsAsync(Guid userId)
    {
        User? user = await _manager.Users.GetUserAsync(userId, true);
        if (user == null)
            return Result<AlbumDTO[]>.Fail("User not found");

        var albums = user.FavoriteAlbums.Select(AlbumDTO.FromAlbum).ToArray();
        return Result<AlbumDTO[]>.Ok(albums);
    }

    public async Task<PagedResult<AlbumDTO>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake)
    {
        return (await _manager.Albums.GetMatchingAlbumsAsync(title, toSkip, toTake))
            .Map(a => AlbumDTO.FromAlbum(a));
    }

    public async Task<Result> AddAlbumToFavoritesAsync(Guid albumId, Guid userId)
    {
        Album? album = await _manager.Albums.GetAlbumAsync(albumId);
        if (album == null)
            return Result.Fail("Album not found");

        User? user = await _manager.Users.GetUserAsync(userId, true);
        if (user == null)
            return Result.Fail("User not found");

        if (user.AddAlbumToFavorites(album))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Album already in favorites");
    }

    public async Task<Result> RemoveAlbumFromFavoritesAsync(Guid albumId, Guid userId)
    {
        Album? album = await _manager.Albums.GetAlbumAsync(albumId);
        if (album == null)
            return Result.Fail("Album not found");

        User? user = await _manager.Users.GetUserAsync(userId, true);
        if (user == null)
            return Result.Fail("User not found");

        if (user.RemoveAlbumFromFavorites(album))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Album not in favorites");
    }

    public async Task<Result> RemoveAlbumAsync(Guid id)
    {
        if (await _manager.Albums.RemoveAlbumAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Album not found");
    }

    public async Task<Result<AlbumDetailDTO?>> GetAlbumDetailAsync(Guid id)
    {
        Album? album = await _manager.Albums.GetAlbumDetailAsync(id);
        if (album != null)
        {
            string[] tags = await _manager.Albums.GetAlbumTags(id, 5);
            AlbumDetailDTO albumDto = AlbumDetailDTO.FromAlbum(album, tags);
            return Result<AlbumDetailDTO?>.Ok(albumDto);
        }
        else
            return Result<AlbumDetailDTO?>.Fail("Album not found");
    }

    public async Task<Result> UpdateAlbumAsync(UpdateAlbumRequest patch)
    {
        if (patch.Tracks == null || patch.Tracks.Length == 0)
            return Result.Fail("At least one track is required");
        Album? album = await _manager.Albums.GetAlbumDetailAsync(patch.Id, true);
        if (album == null) return Result.Fail("Album not found");
        if (!album.SetTitle(patch.Title)) return Result.Fail("Empty title provided");
        if (!album.SetImageUrl(patch.ImageUrl)) return Result.Fail("Wrong image url");
        if (!album.SetReleaseYear(patch.ReleaseYear)) return Result.Fail("Invalid release year provided");
        ArgumentNullException.ThrowIfNull(patch.ArtistName, nameof(patch.ArtistName));
        Artist artist = await _manager.Artists.EnsureArtistCreated(patch.ArtistName);
        if (!album.SetArtist(artist)) return Result.Fail("Invalid artist provided");

        var requestTrackIds = patch.Tracks.Select(t => t.Id).ToHashSet();

        var tracksToRemove = album.Tracks.Where(t => !requestTrackIds.Contains(t.Id)).ToList();
        foreach (var track in tracksToRemove)
        {
            album.Tracks.Remove(track);
            _manager.Tracks.RemoveTrack(track);
        }

        foreach (UpdateTrackInAlbumRequest trackInfo in patch.Tracks)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(trackInfo.Title, nameof(trackInfo.Title));

            var others = new List<Artist>();
            foreach (string name in trackInfo.Others ?? Array.Empty<string>())
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(trackInfo.Others));
                others.Add(await _manager.Artists.EnsureArtistCreated(name));
            }

            Track? existing = null;
            if (trackInfo.Id != null)
                existing = album.Tracks.FirstOrDefault(t => t.Id == trackInfo.Id);

            if (existing != null)
            {
                if (!existing.SetTitle(trackInfo.Title)) return Result.Fail("Empty title provided");
                existing.SetNumberInTheAlbum(trackInfo.NumberInTheAlbum);
                if (!existing.SetOthers(others)) return Result.Fail("Invalid others provided");
            }
            else
            {
                var newTrack = new Track(trackInfo.Title, trackInfo.NumberInTheAlbum, album, album.Artist, others);
                album.AddTrack(newTrack);
            }
        }

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }
}