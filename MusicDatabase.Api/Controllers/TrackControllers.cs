using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using MusicDatabase.Common;
using MusicDatabase.Api;

[ApiController]
[Route("/[controller]")]
public class TrackController(TrackService tracks) : ControllerBase
{
    private readonly TrackService _tracks = tracks;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        var result = await _tracks.GetTrackDetailAsync(id);
        return result.ToActionResult(this);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string title = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        PagedResult<TrackDTO> found = await _tracks.GetMatchingTracksAsync(title, toSkip, toTake);
        return Ok(found);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromBody] UpdateTrackRequest patch)
    {
        if (patch == null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(patch.Title))
            return BadRequest("Title is required.");

        var result = await _tracks.UpdateTrackAsync(patch);
        return result.ToActionResult(this);
    }

    [HttpPut("addtag")]
    [Authorize]
    public async Task<IActionResult> AddTag([FromBody] AddTagOnTrackRequest request)
    {
        if (request.TagName.IsWhiteSpace()) return BadRequest("Empty tag provided");

        var result = await _tracks.AddTagOnTrackAsync(request);
        return result.ToActionResult(this);
    }

    [HttpGet("mytags")]
    [Authorize]
    public async Task<IActionResult> GetUserTags([FromQuery] Guid trackId, Guid userId)
    {
        var result = await _tracks.GetUserTrackTagsAsync(trackId, userId);
        return result.ToActionResult(this);
    }
    
    [HttpGet("tags")]
    public async Task<IActionResult> GetUserTags(
        [FromQuery] Guid trackId,
        [FromQuery, Range(0, 20)] int numberOfTags)
    {
        var result = await _tracks.GetTrackTagsAsync(trackId, numberOfTags);
        return result.ToActionResult(this);
    }
}