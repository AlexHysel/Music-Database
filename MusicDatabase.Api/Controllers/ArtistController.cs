using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using MusicDatabase.Common;
using MusicDatabase.Api;

[ApiController]
[Route("/[controller]")]
public class ArtistController(ArtistService artists, TrackService tracks) : ControllerBase
{
    private readonly ArtistService _artists = artists;
    private readonly TrackService _tracks = tracks;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        var result = await _artists.GetArtistDetailAsync(id);
        return result.ToActionResult(this);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string name = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        PagedResult<ArtistDTO> found = await _artists.GetMatchingArtistsAsync(name, toSkip, toTake);
        return Ok(found);
    }

    [HttpGet("tracks")]
    public async Task<IActionResult> GetTracks(
        [FromQuery] Guid id,
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        var result = await _tracks.GetArtistTracksAsync(id, toSkip, toTake);
        return result.ToActionResult(this);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await _artists.RemoveArtistAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromBody] UpdateArtistRequest patch)
    {
        var result = await _artists.UpdateArtistAsync(patch);
        return result.ToActionResult(this);
    }
}