using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Core;
using MusicDatabase.Contracts;
using System.ComponentModel.DataAnnotations;
using MusicDatabase.Common;

[ApiController]
[Route("/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly Orchestrator _orchestrator;

    public AlbumController(Orchestrator orchestrator) => _orchestrator = orchestrator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<AlbumDetailDTO?> result = await _orchestrator.GetAlbumAsync(id);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string title = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        PagedResult<AlbumDTO> found = await _orchestrator.GetMatchingAlbumsAsync(title, toSkip, toTake);
        return Ok(found);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _orchestrator.RemoveAlbumAsync(id);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromBody] AlbumDTO patch)
    {
        Result result = await _orchestrator.UpdateAlbumAsync(patch);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }
}