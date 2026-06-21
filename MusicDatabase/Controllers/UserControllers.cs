using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/[controller]")]
public class UserController : ControllerBase
{
    private readonly Orchestrator _orchestrator;

    public UserController(Orchestrator orchestrator) => _orchestrator = orchestrator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<UserDTO> result = await _orchestrator.GetUserAsync(id);
        if (result.Success)
        {
            return Ok(result.Data);
        }
        else
        {
            return NotFound(result.Message);
        }
    }
}