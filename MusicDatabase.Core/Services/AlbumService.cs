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

    public async Task<Result> CreateAlbumAsync(AddAlbumRequest info, CancellationToken ctg = default)
    {
        if (info.Tracks.Count() == 0) return Result.Validation("Album.NoTracks", "Album must have at least 1 Track.");

        if (await _manager.Artists.ArtistExistsAsync(info.ArtistName, ctg))
            if (await _manager.Artists.ArtistHasAlbumAsync(info.ArtistName, info.Title, ctg))
                return Result.Conflict("Album.TitleDuplicate", "Album with this title already exists.");
        
        Artist artist = await _manager.Artists.EnsureArtistCreated(info.ArtistName, ctg);
        Album album = new(info.Title, info.ReleaseYear, artist, info.ImageUrl);

        int n = 1;
        foreach (AddTrackRequest trackInfo in info.Tracks)
        {
            List<Artist> others = [];
            if (trackInfo.Others != null)
                foreach (string name in trackInfo.Others)
                    if (!string.IsNullOrEmpty(name))
                        others.Add(await _manager.Artists.EnsureArtistCreated(name, ctg));
            Track track = new(trackInfo.Title, n++, album, artist, others);
            album.Tracks.Add(track);
        }

        await _manager.Albums.CreateAlbumAsync(album, ctg);
        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AddTagOnAlbumAsync(AddTagOnAlbumRequest request, CancellationToken ctg = default)
    {
        if (!await _manager.Albums.AlbumExistsAsync(request.AlbumId, ctg))
            return Result.NotFound("Album.NotFound", "Album with this id does not exist");

        User? user = await _manager.Users.GetUserAsync(request.UserId, true, ctg);
        if (user == null) return Result.NotFound("User.NotFound", "User does not exist.");

        Tag tag = await _manager.Tags.EnsureTagCreatedAsync(request.TagName);

        if (await _manager.Tags.VoteForAlbumTagAsync(request.AlbumId, tag, user, ctg))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Conflict("AlbumTag.VoteDuplicate","You've already added this tag.");
    }

    public async Task<Result<List<TagDetailDTO>>> GetAlbumTagsAsync(Guid albumId, int numberOfTags, CancellationToken ctg = default)
    {
        if (!await _manager.Albums.AlbumExistsAsync(albumId, ctg))
            return Result<List<TagDetailDTO>>.NotFound("Album.NotFound","Album not found");

        var tags = await _manager.Tags.GetAlbumTagStatsAsync(albumId, numberOfTags, ctg);
        return Result<List<TagDetailDTO>>.Ok(tags.Select(t => new TagDetailDTO(t.Tag.Name, t.VoteCount)).ToList());
    }

    public async Task<Result<List<string>>> GetUserAlbumTagsAsync(Guid albumId, Guid userId, CancellationToken ctg = default)
    {
        if (!await _manager.Albums.AlbumExistsAsync(albumId, ctg))
            return Result<List<string>>.NotFound("Album.NotFound", "Album not found");

        User? user = await _manager.Users.GetUserAsync(userId, false, ctg);
        if (user == null)
            return Result<List<string>>.NotFound("User.NotFound", "User not found");

        var tags = await _manager.Tags.GetUserAlbumTagsAsync(albumId, user, ctg);
        return Result<List<string>>.Ok(tags.Select(t => t.Name).ToList());
    }

    public async Task<Result> RemoveTagOnAlbumAsync(RemoveTagOnAlbumRequest request, CancellationToken ctg = default)
    {
        if (string.IsNullOrWhiteSpace(request.TagName))
            return Result.Validation("Tag.Empty", "Tag name cannot be empty.");

        if (!await _manager.Albums.AlbumExistsAsync(request.AlbumId, ctg))
            return Result.NotFound("Album.NotFound", "Album not found");

        User? user = await _manager.Users.GetUserAsync(request.UserId, true, ctg);
        if (user == null)
            return Result.NotFound("User.NotFound", "User not found");

        Tag tag = await _manager.Tags.EnsureTagCreatedAsync(request.TagName, ctg);
        if (await _manager.Tags.RemoveAlbumTagVoteAsync(request.AlbumId, tag, user, ctg))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }

        return Result.NotFound("AlbumTag.NotFound", "You have not added this tag to this album.");
    }

    public async Task<PagedResult<AlbumDTO>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake, CancellationToken ctg = default)
    {
        return (await _manager.Albums.GetMatchingAlbumsAsync(title, toSkip, toTake, ctg))
            .Map(a => AlbumDTO.FromAlbum(a));
    }

    public async Task<Result> RemoveAlbumAsync(Guid id, CancellationToken ctg = default)
    {
        if (await _manager.Albums.RemoveAlbumAsync(id, ctg))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.NotFound("", "Album not found");
    }

    public async Task<Result<AlbumDetailDTO?>> GetAlbumDetailAsync(Guid id, CancellationToken ctg = default)
    {
        Album? album = await _manager.Albums.GetAlbumDetailAsync(id, false, ctg);
        if (album != null)
        {
            AlbumDetailDTO albumDto = AlbumDetailDTO.FromAlbum(album);
            var tags = await GetAlbumTagsAsync(id, 20, ctg);
            if (tags.IsSuccess)
                albumDto = albumDto with { Tags = tags.Value.ToArray() };
            return Result<AlbumDetailDTO?>.Ok(albumDto);
        }
        else
            return Result<AlbumDetailDTO?>.NotFound("Album.NotFound", "Album not found");
    }

    public async Task<Result> UpdateAlbumAsync(UpdateAlbumRequest patch, CancellationToken ctg = default)
    {
        if (patch.Tracks == null || patch.Tracks.Length == 0)
            return Result.Validation("Album.NoTracks", "Album must have at least 1 Track");

        if (!Album.ValidateTitle(patch.Title))
            return Result.Validation("Album.InvalidTitle", "Empty title provided");

        if (!Album.ValidateReleaseYear(patch.ReleaseYear))
            return Result.Validation("Album.InvalidReleaseYear", "Invalid release year provided");

        Album? album = await _manager.Albums.GetAlbumDetailAsync(patch.Id, true, ctg);
        if (album == null)
            return Result.NotFound("Album.NotFound", "Album not found");
            
        album.SetTitle(patch.Title);
        album.SetImageUrl(patch.ImageUrl);
        album.SetReleaseYear(patch.ReleaseYear);

        if (!Artist.ValidateName(patch.ArtistName))
            return Result.Validation("Artist.InvalidName", "Invalid artist name provided");

        var artist = await _manager.Artists.EnsureArtistCreated(patch.ArtistName, ctg);
        album.SetArtist(artist);

        var duplicateTrackNumbers = patch.Tracks
            .GroupBy(t => t.NumberInTheAlbum)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateTrackNumbers.Count > 0)
            return Result.Validation("Track.DuplicateNumberInTheAlbum",
                $"Duplicate track numbers in album update: {string.Join(", ", duplicateTrackNumbers)}");

        var requestTrackIds = patch.Tracks.Select(t => t.Id).ToHashSet();

        var tracksToRemove = album.Tracks.Where(t => !requestTrackIds.Contains(t.Id)).ToList();
        foreach (var track in tracksToRemove)
        {
            album.Tracks.Remove(track);
            _manager.Tracks.RemoveTrack(track);
        }

        var reorderedExistingTracks = new List<(Track Track, int FinalNumber)>();

        foreach (UpdateTrackInAlbumRequest trackInfo in patch.Tracks)
        {
            if (trackInfo.NumberInTheAlbum < 1)
                return Result.Validation("Track.WrongNumberInTheAlbum",
                $"Invalid number of the track '{trackInfo.Title}': {trackInfo.NumberInTheAlbum}. Must be a natural number (>=1)");

            var trackArtistName = string.IsNullOrWhiteSpace(trackInfo.ArtistName) ? patch.ArtistName : trackInfo.ArtistName;
            if (!Artist.ValidateName(trackArtistName))
                return Result.Validation("Track.InvalidArtistName",
                    $"Invalid artist name '{trackArtistName}' for track '{trackInfo.Title}'");

            var others = new List<Artist>();
            foreach (string name in trackInfo.Others ?? [])
            {
                if (!Artist.ValidateName(name))
                    return Result.Validation("Track.InvalidOtherName",
                    $"Invalid name of the 'other' artist: {name} on track '{trackInfo.Title}'");

                others.Add(await _manager.Artists.EnsureArtistCreated(name, ctg));
            }

            Track? existing = null;
            if (trackInfo.Id != null)
                existing = album.Tracks.FirstOrDefault(t => t.Id == trackInfo.Id);

            if (existing != null)
            {
                existing.SetTitle(trackInfo.Title);
                existing.SetOthers(others);
                existing.SetArtist(artist);
                reorderedExistingTracks.Add((existing, trackInfo.NumberInTheAlbum));
            }
            else
            {
                var newTrack = new Track(trackInfo.Title, trackInfo.NumberInTheAlbum, album, album.Artist, others);
                album.AddTrack(newTrack);
            }
        }

        if (reorderedExistingTracks.Count > 0)
        {
            for (var i = 0; i < reorderedExistingTracks.Count; i++)
                reorderedExistingTracks[i].Track.SetNumberInTheAlbum(100_000 + i + 1);

            await _manager.SaveChangesAsync();

            foreach (var (track, finalNumber) in reorderedExistingTracks)
                track.SetNumberInTheAlbum(finalNumber);
        }

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }
}