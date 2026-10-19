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
            return Result<TrackDetailDTO>.Fail("Track not found");
        
        List<Tag> tags = await _manager.Tags.GetTrackTagsAsync(id, 7);
        return Result<TrackDetailDTO>.Ok(TrackDetailDTO.FromTrack(track, tags));
    }

    public async Task<Result> UpdateTrackAsync(UpdateTrackRequest patch)
    {
        Track? track = await _manager.Tracks.GetTrackDetailAsync(patch.Id, true);
        if (track == null) return Result.Fail("Track not found");

        if (!track.SetTitle(patch.Title)) return Result.Fail("Empty Title Provided");

        var normalizedOthers = (patch.Others ?? Array.Empty<string>())
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToArray();

        var others = new List<Artist>();
        foreach (string name in normalizedOthers)
            others.Add(await _manager.Artists.EnsureArtistCreated(name));

        if (!track.SetOthers(others)) return Result.Fail("Wrong others provided");

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> AddTagOnTrackAsync(AddTagOnTrackRequest request)
    {
        var tag = await _manager.Tags.EnsureTagCreatedAsync(request.TagName);
        User? user = await _manager.Users.GetUserAsync(request.UserId, true);
        if (user == null) return Result.Fail("User does not exist.");
        
        if (await _manager.Tags.VoteForTrackTagAsync(request.TrackId, tag, user))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("You've already added this tag.");
    }
}