using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}