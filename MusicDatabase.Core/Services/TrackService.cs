using MusicDatabase.Data;
using MusicDatabase.Contracts;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Core.Services;

public class TrackService
{
    private readonly IMusicManager _manager;

    public TrackService(MusicManager manager)
    {
        _manager = manager;
    }

    public async Task<Result<PagedResult<TrackDTO>>> GetArtistTracksAsync(Guid artistId, int toSkip, int toTake)
    {
        PagedResult<Track> tracks = await _manager.Tracks.GetArtistTracksAsync(artistId, toSkip, toTake);
        return Result<PagedResult<TrackDTO>>.Ok(tracks.Map(TrackDTO.FromTrack));
    }

    public async Task<PagedResult<TrackDTO>> GetMatchingTracksAsync(string title, int toSkip, int toTake)
    {
        return (await _manager.Tracks.GetMatchingTracksAsync(title, toSkip, toTake))
            .Map(TrackDTO.FromTrack);
    }
    
    public async Task<Result<TrackDetailDTO>> GetTrackDetailAsync(Guid id)
    {
        Track? track = await _manager.Tracks.GetTrackDetailAsync(id);
        if (track == null)
            return Result<TrackDetailDTO>.NotFound("Track.NotFound", "Track not found");
        
        return Result<TrackDetailDTO>.Ok(TrackDetailDTO.FromTrack(track));
    }

    public async Task<Result<List<Tag>>> GetUserTrackTagsAsync(Guid trackId, Guid userId)
    {
        if (!await _manager.Tracks.TrackExistsAsync(trackId))
            return Result<List<Tag>>.NotFound("Track.NotFound", "Track not found");

        User? user = await _manager.Users.GetUserAsync(userId);
        if (user == null)
            return Result<List<Tag>>.NotFound("User.NotFound", "User not found");

        var tags = await _manager.Tags.GetUserTrackTagsAsync(trackId, user);
        return Result<List<Tag>>.Ok(tags);
    }

    public async Task<Result<List<TagDetailDTO>>> GetTrackTagsAsync(Guid trackId, int numberOfTags)
    {
        if (!await _manager.Tracks.TrackExistsAsync(trackId))
            return Result<List<TagDetailDTO>>.NotFound("Track.NotFound", "Track not found");

        var tags = await _manager.Tags.GetTrackTagStatsAsync(trackId, numberOfTags);
        return Result<List<TagDetailDTO>>.Ok(tags.Select(t => new TagDetailDTO(t.Tag.Name, t.VoteCount)).ToList());
    }

    public async Task<Result> UpdateTrackAsync(UpdateTrackRequest patch)
    {
        //if (!Track.ValidateTitle(patch.Title))
        //    return Result.Validation("Track.InvalidTitle", "Invalid title provided");

        Track? track = await _manager.Tracks.GetTrackDetailAsync(patch.Id, true);
        if (track == null)
            return Result.NotFound("Track.NotFound", "Track not found");

        track.SetTitle(patch.Title);

        var normalizedOthers = (patch.Others ?? Array.Empty<string>())
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToArray();

        var others = new List<Artist>();
        foreach (string name in normalizedOthers)
            others.Add(await _manager.Artists.EnsureArtistCreated(name));

        track.SetOthers(others);

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AddTagOnTrackAsync(AddTagOnTrackRequest request)
    {
        //if (!Tag.ValidateName(request.TagName))
        //    return Result.Validation("Tag.InvalidName", "Invalid tag name provided");

        var tag = await _manager.Tags.EnsureTagCreatedAsync(request.TagName);
        User? user = await _manager.Users.GetUserAsync(request.UserId, true);
        if (user == null)
            return Result.NotFound("User.NotFound", "User not found");
        
        if (await _manager.Tags.VoteForTrackTagAsync(request.TrackId, tag, user))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Conflict("TrackTag.VoteDuplicate", "You've already added this tag");
    }
}