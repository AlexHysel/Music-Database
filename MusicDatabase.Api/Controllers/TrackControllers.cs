using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using System.ComponentModel.DataAnnotations;
using MusicDatabase.Common;

[ApiController]
[Route("/[controller]")]
public class TrackController : ControllerBase
{
    private readonly TrackService _tracks;

    public TrackController(TrackService tracks) => _tracks = tracks;

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

    /*
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _tracks.RemoveTrackAsync(id);
        if (result.Success)
            return NoContent();
        else
            return NotFound(result.Message);
    }
    */

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
            return NotFound(result.Message);
    }
}