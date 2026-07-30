using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] AddTrackRequest info)
    {
        Enum.TryParse(info.Genre, true, out Genre result);
        await _orchestrator.AddTrackAsync(info.Title, info.Artist, info.Others, info.AlbumTitle, result);
        return Created();
    }

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
}