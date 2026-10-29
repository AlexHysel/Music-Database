using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

using MusicDatabase.Data;
using MusicDatabase.Contracts;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Core.Services;

public class UserService
{
    private readonly IMusicManager _manager;
    private readonly IConfiguration _config;

    public UserService(MusicManager manager, IConfiguration config)
    {
        _manager = manager;
        _config = config;
    }

    public async Task<Result> RemoveUserAsync(Guid id)
    {
        if (await _manager.Users.RemoveUserAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.NotFound("User.NotFound", "User not found");
    }

    public async Task<PagedResult<UserDTO>> GetMatchingUsersAsync(string name, int toSkip, int toTake)
    {
        return (await _manager.Users.GetMatchingUsersAsync(name, toSkip, toTake))
            .Map(u => UserDTO.FromUser(u));
    }

    public async Task<Result<UserDTO>> GetUserAsync(Guid id)
    {
        User? user = await _manager.Users.GetUserAsync(id);
        if (user == null)
            return Result<UserDTO>.NotFound("User.NotFound", "User not found");
        else
            return Result<UserDTO>.Ok(UserDTO.FromUser(user));
    }

    public async Task<Result<UserDetailDTO>> GetUserDetailAsync(Guid id)
    {
        User? user = await _manager.Users.GetUserDetailAsync(id);
        if (user == null)
            return Result<UserDetailDTO>.NotFound("User.NotFound", "User not found");
        else
            return Result<UserDetailDTO>.Ok(UserDetailDTO.FromUser(user));
    }

    //better use DTO here as a paramater
    public async Task<Result> CreateUserAsync(SignUpRequest request)
    {
        //if (!User.ValidateName(request.Username))
        //    return Result.Validation("User.InvalidName", "Invalid name provided");

        if (await _manager.Users.CreateUserAsync(request.Username, request.Password))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Conflict("User", "User already exists");
        
    }

    public async Task<AuthDTO?> LogInAsync(string name, string password)
    {
        AuthDTO? auth = null;
        User? user = await _manager.Users.AuthenticateUser(name, password);
        if (user != null)
        {
            string role = user.Role.ToString();
            string key = _config["Jwt:Key"]!;
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                ]),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = "MusicDatabase",
                Audience = "MusicDatabase",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            auth = new AuthDTO(tokenString);
        }
        return auth;
    }
}