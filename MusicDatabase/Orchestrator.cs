// BUSINESS LOGIC LAYER
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class Orchestrator
{
    private readonly MusicManager _manager;
    private readonly IConfiguration _config;

    public Orchestrator(MusicManager manager, IConfiguration config) {
        _manager = manager;
        _config = config;
    }

    public async Task<SearchResultDTO> Search(string title)
    {
        ArtistDTO[] artists = await _manager.GetArtists().Where(a => a.Name.Contains(title))
            .Select(a => ArtistDTO.FromArtist(a)).ToArrayAsync();
        AlbumDTO[] albums = await _manager.GetAlbums().Where(a => a.Title.Contains(title))
            .Select(a => AlbumDTO.FromAlbum(a)).ToArrayAsync();
        TrackDTO[] tracks = await _manager.GetTracks().Where(a => a.Title.Contains(title))
            .Select(t => TrackDTO.FromTrack(t)).ToArrayAsync();
        UserDTO[] users = await _manager.GetUsers().Where(u => u.Name.Contains(title))
            .Select(u => UserDTO.FromUser(u)).ToArrayAsync();
        return new SearchResultDTO(artists, albums, tracks, users);
    }

    //TRACK
    public async Task<Result> RemoveTrackAsync(Guid id)
    {
        if (await _manager.RemoveTrackAsync(t => t.Id == id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Track not found");
    }

    public async Task<TrackDetailDTO[]> GetTracksAsync(int size, int page, Expression<Func<Track, bool>> filter)
    {
        IQueryable<TrackDetailDTO> request = _manager.GetTracks().Where(filter)
            .Select(t => TrackDetailDTO.FromTrack(t));
        return await request.Skip((page - 1) * size).Take(size).ToArrayAsync();
    }

    public async Task<TrackDetailDTO[]> GetTracksAsync(Expression<Func<Track, bool>> filter)
    {
        IQueryable<TrackDetailDTO> request = _manager.GetTracks().Where(filter)
            .Select(t => TrackDetailDTO.FromTrack(t));
        return await request.ToArrayAsync();
    }

    public async Task<TrackDetailDTO[]> GetTracksAsync(int size, int page)
    {
        IQueryable<TrackDetailDTO> request = _manager.GetTracks()
            .Select(t => TrackDetailDTO.FromTrack(t));
        return await request.Skip((page - 1) * size).Take(size).ToArrayAsync();
    }

    public async Task AddTrackAsync(string title, string artistName, string[]? others, string albumTitle, Genre genre)
    {
        Album album = await _manager.EnsureAlbumCreated(albumTitle, await _manager.EnsureArtistCreated(artistName));

        Artist artist = await _manager.EnsureArtistCreated(artistName);
        List<Artist> artists = [];
        if (others != null)
            foreach (string name in others)
                if (!string.IsNullOrEmpty(name))
                    artists.Add(await _manager.EnsureArtistCreated(name));

        Track track = new() {Title = title, Artist = artist, Album = album, Others = artists, Genre = genre};
        await _manager.AddTrackAsync(track);
        await _manager.SaveChangesAsync();
    }

    public async Task<Result<TrackDetailDTO>> GetTrackAsync(Guid id)
    {
        Track? track = await _manager.GetTracks()
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (track == null)
        {
            return Result<TrackDetailDTO>.Fail("Track not found");
        }
        else
        {
            return Result<TrackDetailDTO>.Ok(TrackDetailDTO.FromTrack(track));
        }
    }

    //ALBUM
    public async Task<Result> RemoveAlbumAsync(Guid id)
    {
        if (await _manager.RemoveAlbumAsync(a => a.Id == id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Album not found");
    }

    public async Task<AlbumDTO[]> GetAlbumsAsync(int size, int page, Expression<Func<Album, bool>> filter)
    {
        var request = _manager.GetAlbums().Where(filter)
            .Select(a => AlbumDTO.FromAlbum(a));
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public async Task<AlbumDTO[]> GetAlbumsAsync(int size, int page)
    {
        var request = _manager.GetAlbums()
            .Select(a => AlbumDTO.FromAlbum(a));
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }
    
    public Task<AlbumDTO[]> GetAlbumsAsync(Expression<Func<Album, bool>> filter)
    {
        return _manager.GetAlbums().Where(filter).Select(a => AlbumDTO.FromAlbum(a)).ToArrayAsync();
    }

    public async Task<Result<AlbumDetailDTO?>> GetAlbumAsync(Expression<Func<Album, bool>> filter)
    {
        Album? album = await _manager.GetAlbumAsync(filter);
        if (album != null)
        {
            AlbumDetailDTO albumDto = AlbumDetailDTO.FromAlbum(album);
            return Result<AlbumDetailDTO?>.Ok(albumDto);
        }
        else
            return Result<AlbumDetailDTO?>.Fail("Album not found");
    }

    //ARTIST
    public async Task<Result> RemoveArtistAsync(Guid id)
    {
        if (await _manager.RemoveArtistAsync(a => a.Id == id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Artist not found");
    }

    public async Task<ArtistDTO[]> GetArtistsAsync(int size, int page)
    {
        var request = _manager.GetArtists().Select(a => new ArtistDTO(a.Name, a.Id.ToString()));
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public async Task<Result<ArtistDetailDTO>> GetArtistAsync(Guid id)
    {
        Artist? artist = await _manager.GetArtists()
            .Include(a => a.Tracks)
            .Include(a => a.Albums)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (artist == null)
            return Result<ArtistDetailDTO>.Fail("Artist not found");
        else
            return Result<ArtistDetailDTO>.Ok(ArtistDetailDTO.FromArtist(artist));
    }

    public async Task<ArtistDTO[]> GetArtistsAsync(int size, int page, Expression<Func<Artist, bool>> filter)
    {
        var request = _manager.GetArtists().Where(filter).Select(a => new ArtistDTO(a.Name, a.Id.ToString()));
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public Task<ArtistDTO[]> GetArtistsAsync(Expression<Func<Artist, bool>> filter)
    {
        return _manager.GetArtists().Where(filter)
            .Select(a => new ArtistDTO(a.Name, a.Id.ToString())).ToArrayAsync();
    }

    //USER
    public async Task<Result> RemoveUserAsync(Guid id)
    {
        if (await _manager.RemoveUserAsync(u => u.Id == id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("User not found");
    }

    public async Task<UserDTO[]> GetUsersAsync(int size, int page)
    {
        var request = _manager.GetUsers()
            .Select(u => new UserDTO(u.Name, u.Role.ToString(), u.Id.ToString()));
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public Task<UserDTO[]> GetUsersAsync(Expression<Func<User, bool>> filter)
    {
        return _manager.GetUsers().Where(filter)
            .Select(u => new UserDTO(u.Name, u.Role.ToString(), u.Id.ToString())).ToArrayAsync();
    }

    public async Task<Result<UserDTO>> GetUserAsync(Guid id)
    {
        User? user = await _manager.GetUserAsync(u => u.Id == id);
        if (user == null)
            return Result<UserDTO>.Fail("User not found");
        else
            return Result<UserDTO>.Ok(UserDTO.FromUser(user));
    }
    
    public async Task<Result> AddUserAsync(string name, string role, string password)
    {
        if (await _manager.HasUserAsync(name))
            return Result.Fail("User with this name already exists");
        else
        {
            await _manager.AddUserAsync(name, Enum.Parse<UserRole>(role), password);
            return Result.Ok();
        }
    }

    public async Task<AuthDTO?> LogInAsync(string name, string password)
    {
        AuthDTO? auth = null;
        string key = _config["Jwt:Key"]!;
        Console.WriteLine($"Login attempt for user: {name}");
        if (await _manager.AuthenticateUser(name, password))
        {
            Console.WriteLine("User authenticated successfully");
            string role = (await _manager.GetUserAsync(u => u.Name == name))!.Role.ToString();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role)
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
            Console.WriteLine($"Token generated: {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");
            auth = new AuthDTO(tokenString);
        }
        else
        {
            Console.WriteLine("User authentication failed");
        }
        return auth;
    }
}