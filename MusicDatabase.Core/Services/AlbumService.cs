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

    public async Task<Result> CreateAlbumAsync(AddAlbumRequest info)
    {
        if (info.Tracks.Count() == 0) return Result.Fail("Album without tracks can't exists");
        
        Artist artist = await _manager.Artists.EnsureArtistCreated(info.ArtistName);
        Album album = new(info.Title, info.ReleaseYear, artist, info.ImageUrl);

        int n = 1;
        foreach (AddTrackRequest trackInfo in info.Tracks)
        {
            List<Artist> others = [];
            if (trackInfo.Others != null)
                foreach (string name in trackInfo.Others)
                    if (!string.IsNullOrEmpty(name))
                        others.Add(await _manager.Artists.EnsureArtistCreated(name));
            Track track = new(trackInfo.Title, n++, album, artist, others);
            album.Tracks.Add(track);
        }

        await _manager.Albums.CreateAlbumAsync(album);
        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AddTagOnAlbumAsync(AddTagOnAlbumRequest request)
    {
        if (!await _manager.Albums.AlbumExistsAsync(request.AlbumId))
            return Result.Fail("Album with this id does not exist");

        User? user = await _manager.Users.GetUserAsync(request.UserId, true);
        if (user == null) return Result.Fail("User does not exist.");

        Tag tag = await _manager.Tags.EnsureTagCreatedAsync(request.TagName);

        if (await _manager.Tags.VoteForAlbumTagAsync(request.AlbumId, tag, user))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("You've already added this tag.");
    }

    public async Task<Result<List<TagDetailDTO>>> GetAlbumTagsAsync(Guid albumId, int numberOfTags)
    {
        if (!await _manager.Albums.AlbumExistsAsync(albumId)) return Result<List<TagDetailDTO>>.Fail("Album does not exist");

        var tags = await _manager.Tags.GetAlbumTagStatsAsync(albumId, numberOfTags);
        return Result<List<TagDetailDTO>>.Ok(tags.Select(t => new TagDetailDTO(t.Tag.Name, t.VoteCount)).ToList());
    }

    public async Task<Result<List<string>>> GetUserAlbumTagsAsync(Guid albumId, Guid userId)
    {
        if (!await _manager.Albums.AlbumExistsAsync(albumId)) return Result<List<string>>.Fail("Album does not exist");
        User? user = await _manager.Users.GetUserAsync(userId);
        if (user == null) return Result<List<string>>.Fail("User does not exist");

        var tags = await _manager.Tags.GetUserAlbumTagsAsync(albumId, user);
        return Result<List<string>>.Ok(tags.Select(t => t.Name).ToList());
    }

    public async Task<PagedResult<AlbumDTO>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake)
    {
        return (await _manager.Albums.GetMatchingAlbumsAsync(title, toSkip, toTake))
            .Map(a => AlbumDTO.FromAlbum(a));
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
            AlbumDetailDTO albumDto = AlbumDetailDTO.FromAlbum(album);
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
            foreach (string name in trackInfo.Others ?? [])
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
                if (!existing.SetArtist(artist)) return Result.Fail("Wrong artist provided");
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