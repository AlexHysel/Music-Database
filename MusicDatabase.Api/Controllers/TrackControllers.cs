using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Core;
using MusicDatabase.Contracts;
using MusicDatabase.Data;
using System.ComponentModel.DataAnnotations;
using MusicDatabase.Common;

[ApiController]
[Route("/[controller]")]
public class TrackController : ControllerBase
{
    private readonly Orchestrator _orchestrator;

    public TrackController(Orchestrator orchestrator) => _orchestrator = orchestrator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<TrackDetailDTO> result = await _orchestrator.GetTrackAsync(id);
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
        PagedResult<TrackDTO> found = await _orchestrator.GetMatchingTracksAsync(title, toSkip, toTake);
        return Ok(found);
    }

    /*
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _orchestrator.RemoveTrackAsync(id);
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
        Result result = await _orchestrator.UpdateTrackAsync(patch);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }
}