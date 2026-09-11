using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

// BUSINESS LOGIC LAYER
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
        ArtistDTO[] artists = (await _manager.GetMatchingArtistsAsync(title))
            .Select(a => ArtistDTO.FromArtist(a)).ToArray();
        AlbumDTO[] albums = (await _manager.GetMatchingAlbumsAsync(title))
            .Select(a => AlbumDTO.FromAlbum(a)).ToArray();
        TrackDTO[] tracks = (await _manager.GetMatchingTracksAsync(title))
            .Select(t => TrackDTO.FromTrack(t)).ToArray();
        UserDTO[] users = (await _manager.GetMatchingUsersAsync(title))
            .Select(u => UserDTO.FromUser(u)).ToArray();
        return new SearchResultDTO(artists, albums, tracks, users);
    }

    //TRACK
    public async Task<Result<TrackDTO[]>> GetFavoriteTracksAsync(Guid userId)
    {
        User? user = await _manager.GetTrackedUserAsync(userId);
        if (user == null)
            return Result<TrackDTO[]>.Fail("User not found");
        else
            return Result<TrackDTO[]>.Ok(user.GetFavoriteTracks().Select(t => TrackDTO.FromTrack(t)).ToArray());
    }
    public async Task<Result> AddTrackToFavoritesAsync(Guid trackId, Guid userId)
    {
        if (await _manager.AddTrackToFavoritesAsync(userId, trackId))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Track already in favorites");
    }

    public async Task<Result> RemoveTrackFromFavoritesAsync(Guid trackId, Guid userId)
    {
        if (await _manager.RemoveTrackFromFavoritesAsync(userId, trackId))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Track not in favorites");
    }

    public async Task<Result> RemoveTrackAsync(Guid id)
    {
        if (await _manager.RemoveTrackAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Track not found");
    }

    public async Task AddTrackAsync(string title, string artistName, string[]? others, string albumTitle, Genre genre)
    {
        Artist artist = await _manager.EnsureArtistCreated(artistName);
        Album album = await _manager.EnsureAlbumCreated(albumTitle, artist);
        
        List<Artist> artists = [];
        if (others != null)
            foreach (string name in others)
                if (!string.IsNullOrEmpty(name))
                    artists.Add(await _manager.EnsureArtistCreated(name));

        Track track = new(title, album, artist, artists, genre);
        await _manager.AddTrackAsync(track);
        await _manager.SaveChangesAsync();
    }

    public async Task<Result<TrackDetailDTO>> GetTrackAsync(Guid id)
    {
        Track? track = await _manager.GetTrackDetailAsync(id);
        if (track == null)
            return Result<TrackDetailDTO>.Fail("Track not found");
        else
            return Result<TrackDetailDTO>.Ok(TrackDetailDTO.FromTrack(track));
    }

    public async Task<Result> UpdateTrackAsync(TrackUpdateDTO patch)
    {
        /*
        IMPORTANT: This code works since the MusicDb is SCOPED, but adding one more SaveChanges
        in the same HTML request can cause some problems.
        */
        Track? track = await _manager.GetTrackedTrackAsync(Guid.Parse(patch.Id));
        if (track == null) return Result.Fail("Track not found");

        if (!track.SetTitle(patch.Title)) return Result.Fail("Empty Title Provided");

        if (!Enum.TryParse(patch.Genre, true, out Genre genre)) return Result.Fail("Wrong genre provided");
        track.SetGenre(genre);

        Artist artist = await _manager.EnsureArtistCreated(patch.ArtistName);
        if (!track.SetArtist(artist)) return Result.Fail("Wrong artist provided");
        
        Album album = await _manager.EnsureAlbumCreated(patch.AlbumTitle, artist);
        if (!track.SetAlbum(album)) return Result.Fail("Wrong album provided");

        var others = new List<Artist>();
        foreach (string name in patch.OthersNames)
            if (!string.IsNullOrEmpty(name))
                others.Add(await _manager.EnsureArtistCreated(name));       
        if (!track.SetOthers(others)) return Result.Fail("Wrong others provided");

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    //ALBUM
    public async Task<Result<AlbumDTO[]>> GetFavoriteAlbumsAsync(Guid userId)
    {
        User? user = await _manager.GetTrackedUserAsync(userId);
        if (user == null)
            return Result<AlbumDTO[]>.Fail("User not found");

        var albums = user.GetFavoriteAlbums().Select(a => AlbumDTO.FromAlbum(a)).ToArray();
        return Result<AlbumDTO[]>.Ok(albums);
    }

    public async Task<Result> AddAlbumToFavoritesAsync(Guid albumId, Guid userId)
    {
        if (await _manager.AddAlbumToFavoritesAsync(userId, albumId))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Album already in favorites");
    }

    public async Task<Result> RemoveAlbumFromFavoritesAsync(Guid albumId, Guid userId)
    {
        if (await _manager.RemoveAlbumFromFavoritesAsync(userId, albumId))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Album not in favorites");
    }

    public async Task<Result> RemoveAlbumAsync(Guid id)
    {
        if (await _manager.RemoveAlbumAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Album not found");
    }

    public async Task<Result<AlbumDetailDTO?>> GetAlbumAsync(Guid id)
    {
        Album? album = await _manager.GetAlbumAsync(id);
        if (album != null)
        {
            AlbumDetailDTO albumDto = AlbumDetailDTO.FromAlbum(album);
            return Result<AlbumDetailDTO?>.Ok(albumDto);
        }
        else
            return Result<AlbumDetailDTO?>.Fail("Album not found");
    }

    public async Task<Result> UpdateAlbumAsync(AlbumDTO patch)
    {
        /*
        IMPORTANT: This code works since the MusicDb is SCOPED, but adding one more SaveChanges
        in the same HTML request can cause some problems.
        */
        Album? album = await _manager.GetTrackedAlbumAsync(Guid.Parse(patch.Id));
        if (album != null)
        {
            if (!album.SetTitle(patch.Title)) return Result.Fail("Empty title provided");
            if (!album.SetImageUrl(patch.ImageUrl)) return Result.Fail("Wrong image url");
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        else
            return Result.Fail("Album Not Found");
    }

    //ARTIST
    public async Task<Result<ArtistDTO[]>> GetFavoriteArtistsAsync(Guid userId)
    {
        User? user = await _manager.GetTrackedUserAsync(userId);
        if (user == null)
            return Result<ArtistDTO[]>.Fail("User not found");

        var artists = user.GetFavoriteArtists().Select(a => ArtistDTO.FromArtist(a)).ToArray();
        return Result<ArtistDTO[]>.Ok(artists);
    }

    public async Task<Result> AddArtistToFavoritesAsync(Guid userId, Guid artistId)
    {
        if (await _manager.AddArtistToFavoritesAsync(userId, artistId))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist already in favorites");
    }

    public async Task<Result> RemoveArtistFromFavoritesAsync(Guid userId, Guid artistId)
    {
        if (await _manager.RemoveArtistFromFavoritesAsync(userId, artistId))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist not in favorites");
    }

    public async Task<Result> RemoveArtistAsync(Guid id)
    {
        if (await _manager.RemoveArtistAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist not found");
    }

    public async Task<Result> UpdateArtistAsync(ArtistDTO patch)
    {
        /*
        IMPORTANT: This code works since the MusicDb is SCOPED, but adding one more SaveChanges
        in the same HTML request can cause some problems.
        */
        Artist? artist = await _manager.GetTrackedArtistAsync(Guid.Parse(patch.Id));
        if (artist != null)
        {
            if (!artist.SetName(patch.Name)) return Result.Fail("Empty name provided");
            if (!artist.SetImageUrl(patch.ImageUrl)) return Result.Fail("Wrong image url");
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist not found");
    }

    public async Task<Result<ArtistDetailDTO>> GetArtistAsync(Guid id)
    {
        Artist? artist = await _manager.GetArtistDetailAsync(id);
        if (artist == null)
            return Result<ArtistDetailDTO>.Fail("Artist not found");
        else
            return Result<ArtistDetailDTO>.Ok(ArtistDetailDTO.FromArtist(artist));
    }

    //USER
    public async Task<Result> RemoveUserAsync(Guid id)
    {
        if (await _manager.RemoveUserAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("User not found");
    }

    public async Task<Result<UserDTO>> GetUserAsync(Guid id)
    {
        User? user = await _manager.GetUserAsync(id);
        if (user == null)
            return Result<UserDTO>.Fail("User not found");
        else
            return Result<UserDTO>.Ok(UserDTO.FromUser(user));
    }

    public async Task<Result<UserDetailDTO>> GetUserDetailAsync(Guid id)
    {
        User? user = await _manager.GetUserDetailAsync(id);
        if (user == null)
            return Result<UserDetailDTO>.Fail("User not found");
        else
            return Result<UserDetailDTO>.Ok(UserDetailDTO.FromUser(user));
    }
    
    public async Task<Result> AddUserAsync(string name, string role, string password)
    {
        if (await _manager.UserExistsAsync(name))
            return Result.Fail("User with this name already exists");
        await _manager.AddUserAsync(name, Enum.Parse<UserRole>(role), password);
        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<AuthDTO?> LogInAsync(string name, string password)
    {
        AuthDTO? auth = null;
        User? user = await _manager.AuthenticateUser(name, password);
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