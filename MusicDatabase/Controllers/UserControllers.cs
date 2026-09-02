using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _orchestrator.RemoveUserAsync(id);
        if (result.Success)
            return NoContent();
        else
            return NotFound(result.Message);
    }

    [HttpGet("me/favorites/albums")]
    public async Task<IActionResult> GetFavoriteAlbums()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result<AlbumDTO[]> result = await _orchestrator.GetFavoriteAlbumsAsync(userId);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }

    [HttpGet("me/favorites/artists")]
    public async Task<IActionResult> GetFavoriteArtists()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result<ArtistDTO[]> result = await _orchestrator.GetFavoriteArtistsAsync(userId);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }

    [HttpGet("me/favorites/tracks")]
    public async Task<IActionResult> GetFavoriteTracks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result<TrackDTO[]> result = await _orchestrator.GetFavoriteTracksAsync(userId);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }
}