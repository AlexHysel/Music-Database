using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDatabase.Common;
using MusicDatabase.Contracts;
using MusicDatabase.Core.Services;

[ApiController]
[Route("/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _users;
    private readonly AlbumService _albums;
    private readonly ArtistService _artists;
    private readonly TrackService _tracks;

    public UserController(UserService users, AlbumService albums, ArtistService artists, TrackService tracks)
    {
        _users = users;
        _albums = albums;
        _artists = artists;
        _tracks = tracks;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<UserDetailDTO> result = await _users.GetUserDetailAsync(id);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string name = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        PagedResult<UserDTO> found = await _users.GetMatchingUsersAsync(name, toSkip, toTake);
        return Ok(found);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _users.RemoveUserAsync(id);
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

        Result<AlbumDTO[]> result = await _albums.GetFavoriteAlbumsAsync(userId);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }

    [HttpPost("me/favorites/albums")]
    public async Task<IActionResult> AddFavoriteAlbum([FromQuery] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _albums.AddAlbumToFavoritesAsync(id, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpDelete("me/favorites/albums")]
    public async Task<IActionResult> RemoveFavoriteAlbum([FromQuery] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _albums.RemoveAlbumFromFavoritesAsync(id, userId);
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

        Result<ArtistDTO[]> result = await _artists.GetFavoriteArtistsAsync(userId);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound(result.Message);
    }

    [HttpPost("me/favorites/artists")]
    public async Task<IActionResult> AddFavoriteArtist([FromQuery] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _artists.AddArtistToFavoritesAsync(userId, id);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpDelete("me/favorites/artists")]
    public async Task<IActionResult> RemoveFavoriteArtist([FromQuery] Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name;
        if (userIdClaim == null) return Forbid();

        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user id in token");

        Result result = await _artists.RemoveArtistFromFavoritesAsync(userId, id);
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

        Result result = await _tracks.AddTrackToFavoritesAsync(Id, userId);
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

        Result<TrackDTO[]> result = await _tracks.GetFavoriteTracksAsync(userId);
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

        Result result = await _tracks.RemoveTrackFromFavoritesAsync(Id, userId);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }
}