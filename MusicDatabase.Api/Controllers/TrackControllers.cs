using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using MusicDatabase.Common;

[ApiController]
[Route("/[controller]")]
public class TrackController(TrackService tracks) : ControllerBase
{
    private readonly TrackService _tracks = tracks;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<TrackDetailDTO> result = await _tracks.GetTrackDetailAsync(id);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound("Track not found");
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

        Result result = await _tracks.UpdateTrackAsync(patch);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpPut("addtag")]
    [Authorize]
    public async Task<IActionResult> AddTag([FromBody] AddTagOnTrackRequest request)
    {
        if (request.TagName.IsWhiteSpace()) return BadRequest("Empty tag provided");

        Result result = await _tracks.AddTagOnTrackAsync(request);
        if (result.Success)
            return Ok();
        else
            return NotFound(result);
    }

    [HttpGet("mytags")]
    [Authorize]
    public async Task<IActionResult> GetUserTags([FromQuery] Guid trackId, Guid userId)
    {
        var result = await _tracks.GetUserTrackTagsAsync(trackId, userId);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }
    
    [HttpGet("tags")]
    public async Task<IActionResult> GetUserTags(
        [FromQuery] Guid trackId,
        [FromQuery, Range(0, 20)] int numberOfTags)
    {
        var result = await _tracks.GetTrackTagsAsync(trackId, numberOfTags);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }
}