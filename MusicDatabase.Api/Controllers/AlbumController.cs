using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Core;
using MusicDatabase.Contracts;
using System.ComponentModel.DataAnnotations;
using MusicDatabase.Common;

[ApiController]
[Route("/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly Orchestrator _orchestrator;

    public AlbumController(Orchestrator orchestrator) => _orchestrator = orchestrator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        Result<AlbumDetailDTO?> result = await _orchestrator.GetAlbumDetailAsync(id);
        if (result.Success)
            return Ok(result.Data);
        else
            return NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string title = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        PagedResult<AlbumDTO> found = await _orchestrator.GetMatchingAlbumsAsync(title, toSkip, toTake);
        return Ok(found);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] AddAlbumRequest info)
    {
        if (info == null) return BadRequest("Request body is null.");
        if (info.Title == null || info.Title.Trim().Length == 0) return BadRequest("Title is required.");
        if (info.ArtistName == null || info.ArtistName.Trim().Length == 0) return BadRequest("Artist is required.");
        if (info.ReleaseYear < 1900 || info.ReleaseYear > DateTimeOffset.UtcNow.Year + 1) return BadRequest($"Release year must be between 1900 and {DateTimeOffset.UtcNow.Year + 1}.");
        if (info.Tracks == null || info.Tracks.Length == 0) return BadRequest("At least one track is required.");
        if (info.Tracks.Any(t => t.Title == null || t.Title.Trim().Length == 0)) return BadRequest("All tracks must have a title.");
        if (info.Tracks.Any(t => t.Genre == null || t.Genre.Trim().Length == 0)) return BadRequest("All tracks must have a genre.");
        if (info.Tracks.Any(t => t.Others != null && t.Others.Any(o => o == null || o.Trim().Length == 0))) return BadRequest("All other artists must have a name.");

        await _orchestrator.AddAlbumAsync(info);
        return Created();
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