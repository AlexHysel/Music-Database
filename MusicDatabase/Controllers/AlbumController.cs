using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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