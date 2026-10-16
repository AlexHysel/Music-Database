using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using System.ComponentModel.DataAnnotations;
using MusicDatabase.Common;

[ApiController]
[Route("/[controller]")]
public class ArtistController : ControllerBase
{
    private readonly ArtistService _artists;
    private readonly TrackService _tracks;

    public ArtistController(ArtistService artists, TrackService tracks)
    {
        _artists = artists;
        _tracks = tracks;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<ArtistDetailDTO> result = await _artists.GetArtistDetailAsync(id);
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
        PagedResult<ArtistDTO> found = await _artists.GetMatchingArtistsAsync(name, toSkip, toTake);
        return Ok(found);
    }

    [HttpGet("tracks")]
    public async Task<IActionResult> GetTracks(
        [FromQuery] Guid id,
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        Result<PagedResult<TrackDTO>> tracks = await _tracks.GetArtistTracksAsync(id, toSkip, toTake);
        return Ok(tracks.Data);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        Result result = await _artists.RemoveArtistAsync(id);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromBody] UpdateArtistRequest patch)
    {
        Result result = await _artists.UpdateArtistAsync(patch);
        if (result.Success)
            return Ok();
        else
            return NotFound(result.Message);
    }
}