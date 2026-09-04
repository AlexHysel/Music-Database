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
        Result<UserDetailDTO> result = await _orchestrator.GetUserDetailAsync(id);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
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

    // FAVORITE ALBUMS
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

    [HttpPost("me/favorites/albums")]
    public async Task<IActionResult> AddFavoriteAlbum([FromQuery] Guid albumId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _orchestrator.AddAlbumToFavoritesAsync(albumId, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpDelete("me/favorites/albums")]
    public async Task<IActionResult> RemoveFavoriteAlbum([FromQuery] Guid albumId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _orchestrator.RemoveAlbumFromFavoritesAsync(albumId, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    // FAVORITE ARTISTS
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

    [HttpPost("me/favorites/artists")]
    public async Task<IActionResult> AddFavoriteArtist([FromQuery] Guid artistId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _orchestrator.AddArtistToFavoritesAsync(artistId, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpDelete("me/favorites/artists")]
    public async Task<IActionResult> RemoveFavoriteArtist([FromQuery] Guid artistId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _orchestrator.RemoveArtistFromFavoritesAsync(artistId, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    // FAVORITE TRACKS
    [HttpPost("me/favorites/tracks")]
    public async Task<IActionResult> AddFavoriteTrack([FromQuery] Guid Id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _orchestrator.AddTrackToFavoritesAsync(Id, userId);
        if (result.Success)
            return Ok();
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

    [HttpDelete("me/favorites/tracks")]
    public async Task<IActionResult> RemoveFavoriteTrack([FromQuery] Guid Id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _orchestrator.RemoveTrackFromFavoritesAsync(Id, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }
}