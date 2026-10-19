using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MusicDatabase.Common;
using MusicDatabase.Contracts;
using MusicDatabase.Core.Services;

[ApiController]
[Route("/[controller]")]
public class UserController(UserService users) : ControllerBase
{
    private readonly UserService _users = users;

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
}