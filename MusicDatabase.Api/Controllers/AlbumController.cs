using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using MusicDatabase.Api;

[ApiController]
[Route("/[controller]")]
public class AlbumController(AlbumService albums) : ControllerBase
{
    private readonly AlbumService _albums = albums;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        var result = await _albums.GetAlbumDetailAsync(id);
        return result.ToActionResult(this);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string title = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
    {
        var found = await _albums.GetMatchingAlbumsAsync(title, toSkip, toTake);
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
        if (info.Tracks.Any(t => t.Others != null && t.Others.Any(o => o == null || o.Trim().Length == 0))) return BadRequest("All other artists must have a name.");

        var result = await _albums.CreateAlbumAsync(info);
        return result.ToActionResult(this);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await _albums.RemoveAlbumAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put([FromBody] UpdateAlbumRequest patch)
    {
        var result = await _albums.UpdateAlbumAsync(patch);
        return result.ToActionResult(this);
    }

    [HttpPut("addtag")]
    [Authorize]
    public async Task<IActionResult> AddTag([FromBody] AddTagOnAlbumRequest request)
    {
        if (request.TagName.IsWhiteSpace()) return BadRequest("Empty tag provided");

        var result = await _albums.AddTagOnAlbumAsync(request);
        return result.ToActionResult(this);
    }

    [HttpGet("mytags")]
    [Authorize]
    public async Task<IActionResult> GetUserTags([FromQuery] Guid albumId, Guid userId)
    {
        var result = await _albums.GetUserAlbumTagsAsync(albumId, userId);
        return result.ToActionResult(this);
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetUserTags(
        [FromQuery] Guid albumId,
        [FromQuery, Range(0, 20)] int numberOfTags)
    {
        var result = await _albums.GetAlbumTagsAsync(albumId, numberOfTags);
        return result.ToActionResult(this);
    }
}