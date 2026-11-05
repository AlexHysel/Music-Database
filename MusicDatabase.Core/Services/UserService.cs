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

    public async Task<Result> RemoveUserAsync(Guid id, CancellationToken ctg = default)
    {
        if (await _manager.Users.RemoveUserAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.NotFound("User.NotFound", "User not found");
    }

    public async Task<PagedResult<UserDTO>> GetMatchingUsersAsync(string name, int toSkip, int toTake, CancellationToken ctg = default)
    {
        return (await _manager.Users.GetMatchingUsersAsync(name, toSkip, toTake))
            .Map(u => UserDTO.FromUser(u));
    }

    public async Task<Result<UserDTO>> GetUserAsync(Guid id, CancellationToken ctg)
    {
        User? user = await _manager.Users.GetUserAsync(id);
        if (user == null)
            return Result<UserDTO>.NotFound("User.NotFound", "User not found");
        else
            return Result<UserDTO>.Ok(UserDTO.FromUser(user));
    }

    public async Task<Result<UserDetailDTO>> GetUserDetailAsync(Guid id, CancellationToken ctg = default)
    {
        User? user = await _manager.Users.GetUserDetailAsync(id);
        if (user == null)
            return Result<UserDetailDTO>.NotFound("User.NotFound", "User not found");
        else
            return Result<UserDetailDTO>.Ok(UserDetailDTO.FromUser(user));
    }

    public async Task<Result> CreateUserAsync(SignUpRequest request, CancellationToken ctg = default)
    {
        if (!User.ValidateName(request.Username))
            return Result.Validation("User.InvalidName", "Invalid name provided");
        if (!User.ValidateRawPassword(request.Password))
            return Result.Validation("User.InvalidPassword", "Invalid password provided");

        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

        if (await _manager.Users.CreateUserAsync(request.Username, passwordHash))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Conflict("User", "User already exists");
        
    }

    public async Task<Result<AuthDTO>> LogInAsync(string name, string password, CancellationToken ctg = default)
    {
        User? user = await _manager.Users.GetUserAsync(name);
        if (user == null)
            return Result<AuthDTO>.NotFound("User.NotFound", "User not found");

        if (!BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
            return Result<AuthDTO>.Unauthorized("User.Unauthorized", "Wrong password or username");

        string role = user.Role.ToString();
        string key = _config["Jwt__Key"] ?? _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing. Set the 'Jwt__Key' secret before starting the application.");
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
        var auth = new AuthDTO(tokenString);

        return Result<AuthDTO>.Ok(auth);
    }
}