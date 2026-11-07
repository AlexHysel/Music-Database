using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using MusicDatabase.Core.Services;
using MusicDatabase.Contracts;
using MusicDatabase.Api;

[ApiController]
[Route("/[controller]")]
public class AlbumController(AlbumService albums, CacheService cache) : ControllerBase
{
    private readonly AlbumService _albums = albums;
    private readonly CacheService _cache = cache;

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken ctg,
        [FromQuery] Guid id)
    {
        var url = "album/" + id.ToString();
        var album = await _cache.GetCachedResult<AlbumDetailDTO>(url);
        
        if (album == null)
        {
            var result = await _albums.GetAlbumDetailAsync(id, ctg);
            if (result.IsSuccess) await _cache.SetCachedResult(url, result.Value);
            return result.ToActionResult(this);
        }

        return Result<AlbumDetailDTO>.Ok(album).ToActionResult(this);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        CancellationToken ctg,
        [FromQuery] string title = "",
        [FromQuery, Range(0, int.MaxValue)] int toSkip = 0,
        [FromQuery, Range(1, 50)] int toTake = 5)
        
    {
        var found = await _albums.GetMatchingAlbumsAsync(title, toSkip, toTake, ctg);
        return Ok(found);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post(
        CancellationToken ctg,
        [FromBody] AddAlbumRequest info)
    {
        if (info == null) return BadRequest("Request body is null.");
        if (info.Title == null || info.Title.Trim().Length == 0) return BadRequest("Title is required.");
        if (info.ArtistName == null || info.ArtistName.Trim().Length == 0) return BadRequest("Artist is required.");
        if (info.ReleaseYear < 1900 || info.ReleaseYear > DateTimeOffset.UtcNow.Year + 1) return BadRequest($"Release year must be between 1900 and {DateTimeOffset.UtcNow.Year + 1}.");
        if (info.Tracks == null || info.Tracks.Length == 0) return BadRequest("At least one track is required.");
        if (info.Tracks.Any(t => t.Title == null || t.Title.Trim().Length == 0)) return BadRequest("All tracks must have a title.");
        if (info.Tracks.Any(t => t.Others != null && t.Others.Any(o => o == null || o.Trim().Length == 0))) return BadRequest("All other artists must have a name.");

        var result = await _albums.CreateAlbumAsync(info, ctg);
        return result.ToActionResult(this);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        CancellationToken ctg,
        [FromQuery] Guid id)
    {
        var result = await _albums.RemoveAlbumAsync(id, ctg);
        await _cache.DeleteKey($"album/{id}");
        return result.ToActionResult(this);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put(
        CancellationToken ctg,
        [FromBody] UpdateAlbumRequest patch)
    {
        var result = await _albums.UpdateAlbumAsync(patch, ctg);
        return result.ToActionResult(this);
    }

    [HttpPut("addtag")]
    [Authorize]
    public async Task<IActionResult> AddTag(
        CancellationToken ctg,
        [FromBody] AddTagOnAlbumRequest request)
    {
        if (request.TagName.IsWhiteSpace()) return BadRequest("Empty tag provided");

        var result = await _albums.AddTagOnAlbumAsync(request, ctg);
        return result.ToActionResult(this);
    }

    [HttpGet("mytags")]
    [Authorize]
    public async Task<IActionResult> GetUserTags(
        CancellationToken ctg,
        [FromQuery] Guid albumId, Guid userId)
    {
        var result = await _albums.GetUserAlbumTagsAsync(albumId, userId, ctg);
        return result.ToActionResult(this);
    }

    [HttpDelete("mytags")]
    [Authorize]
    public async Task<IActionResult> RemoveUserTag(
        CancellationToken ctg,
        [FromBody] RemoveTagOnAlbumRequest request)
    {
        var result = await _albums.RemoveTagOnAlbumAsync(request, ctg);
        return result.ToActionResult(this);
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetUserTags(
        CancellationToken ctg,
        [FromQuery] Guid albumId,
        [FromQuery, Range(0, 20)] int numberOfTags)
    {
        var result = await _albums.GetAlbumTagsAsync(albumId, numberOfTags, ctg);
        return result.ToActionResult(this);
    }
}