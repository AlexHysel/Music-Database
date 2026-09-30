using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Core;
using MusicDatabase.Contracts;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("/[controller]")]
public class ArtistController : ControllerBase
{
    private readonly Orchestrator _orchestrator;

    public ArtistController(Orchestrator orchestrator) => _orchestrator = orchestrator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<ArtistDetailDTO> result = await _orchestrator.GetArtistAsync(id);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string name = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        ArtistDTO[] found = await _orchestrator.GetMatchingArtistsAsync(name, toSkip, toTake);
        return Ok(found);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _orchestrator.RemoveArtistAsync(id);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromBody] ArtistDTO patch)
    {
        Result result = await _orchestrator.UpdateArtistAsync(patch);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }
}