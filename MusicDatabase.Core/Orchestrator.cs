using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

using MusicDatabase.Data;
using MusicDatabase.Contracts;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Core;

// BUSINESS LOGIC LAYER
public class Orchestrator
{
    private readonly IMusicManager _manager;
    private readonly IConfiguration _config;

    public Orchestrator(MusicManager manager, IConfiguration config) {
        _manager = manager;
        _config = config;
    }

    public async Task<SearchResultDTO> Search(string title)
    {
        PagedResult<ArtistDTO> artists = await GetMatchingArtistsAsync(title, 0, 10);
        PagedResult<AlbumDTO> albums = await GetMatchingAlbumsAsync(title, 0, 10);
        PagedResult<TrackDTO> tracks = await GetMatchingTracksAsync(title, 0, 15);
        PagedResult<UserDTO> users = await GetMatchingUsersAsync(title, 0, 10);
        return new SearchResultDTO(artists, albums, tracks, users);
    }

    //TRACK
    public async Task<Result<PagedResult<TrackDTO>>> GetArtistTracksAsync(Guid artistId, int toSkip, int toTake)
    {
        PagedResult<Track> tracks = await _manager.Tracks.GetArtistTracksAsync(artistId, toSkip, toTake);
        return Result<PagedResult<TrackDTO>>.Ok(tracks.Map(TrackDTO.FromTrack));
    }

    public async Task<Result<TrackDTO[]>> GetFavoriteTracksAsync(Guid userId)
    {
        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result<TrackDTO[]>.Fail("User not found");
        else
            return Result<TrackDTO[]>.Ok(user.FavoriteTracks.Select(TrackDTO.FromTrack).ToArray());
    }

    public async Task<PagedResult<TrackDTO>> GetMatchingTracksAsync(string title, int toSkip, int toTake)
    {
        return (await _manager.Tracks.GetMatchingTracksAsync(title, toSkip, toTake))
            .Map(TrackDTO.FromTrack);
    }

    public async Task<Result> AddTrackToFavoritesAsync(Guid trackId, Guid userId)
    {
        Track? track = await _manager.Tracks.GetTrackAsync(trackId);
        if (track == null)
            return Result.Fail("Track not found");

        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result.Fail("User not found");

        if (user.AddTrackToFavorites(track)) 
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Track already in favorites");
    }

    public async Task<Result> RemoveTrackFromFavoritesAsync(Guid trackId, Guid userId)
    {
        Track? track = await _manager.Tracks.GetTrackAsync(trackId);
        if (track == null)
            return Result.Fail("Track not found");

        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result.Fail("User not found");

        if (user.RemoveTrackFromFavorites(track))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Track not in favorites");
    }

    public async Task<Result<TrackDetailDTO>> GetTrackDetailAsync(Guid id)
    {
        Track? track = await _manager.Tracks.GetTrackDetailAsync(id);
        if (track == null)
            return Result<TrackDetailDTO>.Fail("Track not found");
        else
            return Result<TrackDetailDTO>.Ok(TrackDetailDTO.FromTrack(track));
    }

    public async Task<Result> UpdateTrackAsync(UpdateTrackRequest patch)
    {
        if (string.IsNullOrWhiteSpace(patch.Id) || !Guid.TryParse(patch.Id, out Guid trackId))
            return Result.Fail("Track id is invalid");

        Track? track = await _manager.Tracks.GetTrackedTrackAsync(trackId);
        if (track == null) return Result.Fail("Track not found");

        if (!track.SetTitle(patch.Title)) return Result.Fail("Empty Title Provided");

        if (!Enum.TryParse(patch.Genre, true, out Genre genre))
            return Result.Fail("Wrong genre provided");
        track.SetGenre(genre);

        var others = new List<Artist>();
        foreach (string name in patch.Others ?? Array.Empty<string>())
            if (!string.IsNullOrEmpty(name))
                others.Add(await _manager.Artists.EnsureArtistCreated(name));
        if (!track.SetOthers(others)) return Result.Fail("Wrong others provided");

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    //ALBUM
    public async Task CreateAlbumAsync(AddAlbumRequest info)
    {
        Artist artist = await _manager.Artists.EnsureArtistCreated(info.ArtistName);
        Album album = new(info.Title, info.ReleaseYear, artist, info.ImageUrl);

        int n = 1;
        foreach (AddTrackRequest trackInfo in info.Tracks)
        {
            List<Artist> others = new();
            if (trackInfo.Others != null)
                foreach (string name in trackInfo.Others)
                    if (!string.IsNullOrEmpty(name))
                        others.Add(await _manager.Artists.EnsureArtistCreated(name));
            Track track = new(trackInfo.Title, n++, album, artist, others, Enum.Parse<Genre>(trackInfo.Genre, true));
            album.Tracks.Add(track);
        }

        await _manager.Albums.CreateAlbumAsync(album);
        await _manager.SaveChangesAsync();
    }

    public async Task<Result<AlbumDTO[]>> GetFavoriteAlbumsAsync(Guid userId)
    {
        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result<AlbumDTO[]>.Fail("User not found");

        var albums = user.FavoriteAlbums.Select(AlbumDTO.FromAlbum).ToArray();
        return Result<AlbumDTO[]>.Ok(albums);
    }

    public async Task<PagedResult<AlbumDTO>> GetMatchingAlbumsAsync(string title, int toSkip, int toTake)
    {
        return (await _manager.Albums.GetMatchingAlbumsAsync(title, toSkip, toTake))
            .Map(a => AlbumDTO.FromAlbum(a));
    }

    public async Task<Result> AddAlbumToFavoritesAsync(Guid albumId, Guid userId)
    {
        Album? album = await _manager.Albums.GetAlbumAsync(albumId);
        if (album == null)
            return Result.Fail("Album not found");

        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result.Fail("User not found");

        if (user.AddAlbumToFavorites(album))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Album already in favorites");
    }

    public async Task<Result> RemoveAlbumFromFavoritesAsync(Guid albumId, Guid userId)
    {
        Album? album = await _manager.Albums.GetAlbumAsync(albumId);
        if (album == null)
            return Result.Fail("Album not found");

        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result.Fail("User not found");

        if (user.RemoveAlbumFromFavorites(album))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Album not in favorites");
    }

    public async Task<Result> RemoveAlbumAsync(Guid id)
    {
        if (await _manager.Albums.RemoveAlbumAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Album not found");
    }

    public async Task<Result<AlbumDetailDTO?>> GetAlbumDetailAsync(Guid id)
    {
        Album? album = await _manager.Albums.GetAlbumDetailAsync(id);
        if (album != null)
        {
            AlbumDetailDTO albumDto = AlbumDetailDTO.FromAlbum(album);
            return Result<AlbumDetailDTO?>.Ok(albumDto);
        }
        else
            return Result<AlbumDetailDTO?>.Fail("Album not found");
    }

    public async Task<Result> UpdateAlbumAsync(UpdateAlbumRequest patch)
    {
        if (patch.Tracks == null || patch.Tracks.Length == 0)
            return Result.Fail("At least one track is required");

        Album? album = await _manager.Albums.GetTrackedAlbumDetailAsync(Guid.Parse(patch.Id));
        if (album == null) return Result.Fail("Album not found");

        if (!album.SetTitle(patch.Title)) return Result.Fail("Empty title provided");
        if (!album.SetImageUrl(patch.ImageUrl)) return Result.Fail("Wrong image url");
        if (!album.SetReleaseYear(patch.ReleaseYear)) return Result.Fail("Invalid release year provided");
        
        ArgumentNullException.ThrowIfNull(patch.ArtistName, nameof(patch.ArtistName));
        Artist artist = await _manager.Artists.EnsureArtistCreated(patch.ArtistName);
        if (!album.SetArtist(artist)) return Result.Fail("Invalid artist provided");

        // 1. Собираем ID треков, пришедших в PATCH
        var requestTrackIds = patch.Tracks
            .Where(t => !string.IsNullOrWhiteSpace(t.Id) && Guid.TryParse(t.Id, out _))
            .Select(t => Guid.Parse(t.Id))
            .ToHashSet();

        // 2. УДАЛЕНИЕ: Находим треки, которых НЕТ в запросе, и удаляем из коллекции
        var tracksToRemove = album.Tracks.Where(t => !requestTrackIds.Contains(t.Id)).ToList();
        foreach (var track in tracksToRemove)
        {
            album.Tracks.Remove(track);
            await _manager.Tracks.RemoveTrackAsync(track); // Вызывает Tracks.Remove(track) и пересчитывает номера
        }

        // 3. ОБНОВЛЕНИЕ И ДОБАВЛЕНИЕ
        foreach (UpdateTrackRequest trackInfo in patch.Tracks)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(trackInfo.Title, nameof(trackInfo.Title));
            if (!Enum.TryParse(trackInfo.Genre, true, out Genre genre))
                return Result.Fail("Wrong genre provided");

            var others = new List<Artist>();
            foreach (string name in trackInfo.Others ?? Array.Empty<string>())
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(trackInfo.Others));
                others.Add(await _manager.Artists.EnsureArtistCreated(name));
            }

            Track? existing = null;
            if (!string.IsNullOrWhiteSpace(trackInfo.Id) && Guid.TryParse(trackInfo.Id, out Guid tid))
            {
                existing = album.Tracks.FirstOrDefault(t => t.Id == tid);
            }

            if (existing != null)
            {
                // ОБНОВЛЕНИЕ СУЩЕСТВУЮЩЕГО: Не вызываем AddTrack!
                if (!existing.SetTitle(trackInfo.Title)) return Result.Fail("Empty title provided");
                existing.SetGenre(genre);
                existing.SetNumberInTheAlbum(trackInfo.NumberInTheAlbum);
                if (!existing.SetOthers(others)) return Result.Fail("Invalid others provided");
            }
            else
            {
                // ДОБАВЛЕНИЕ НОВОГО: Создаем объект и добавляем через AddTrack
                var newTrack = new Track(trackInfo.Title, trackInfo.NumberInTheAlbum, album, album.Artist, others, genre);
                album.AddTrack(newTrack);
            }
        }

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    //ARTIST
    public async Task<Result<ArtistDTO[]>> GetFavoriteArtistsAsync(Guid userId)
    {
        User? user = await _manager.Users.GetUserAsync(userId);
        if (user == null)
            return Result<ArtistDTO[]>.Fail("User not found");

        var artists = user.FavoriteArtists.Select(ArtistDTO.FromArtist).ToArray();
        return Result<ArtistDTO[]>.Ok(artists);
    }

    public async Task<PagedResult<ArtistDTO>> GetMatchingArtistsAsync(string name, int toSkip, int toTake)
    {
        return (await _manager.Artists.GetMatchingArtistsAsync(name, toSkip, toTake))
            .Map(a => ArtistDTO.FromArtist(a));
    }

    public async Task<Result> AddArtistToFavoritesAsync(Guid userId, Guid artistId)
    {
        Artist? artist = await _manager.Artists.GetArtistAsync(artistId);
        if (artist == null)
            return Result.Fail("Artist not found");

        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result.Fail("User not found");

        if (user.AddArtistToFavorites(artist))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist already in favorites");
    }

    public async Task<Result> RemoveArtistFromFavoritesAsync(Guid userId, Guid artistId)
    {
        Artist? artist = await _manager.Artists.GetArtistAsync(artistId);
        if (artist == null)
            return Result.Fail("Artist not found");
        
        User? user = await _manager.Users.GetTrackedUserAsync(userId);
        if (user == null)
            return Result.Fail("User not found");

        if (user.RemoveArtistFromFavorites(artist))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist not in favorites");
    }

    public async Task<Result> RemoveArtistAsync(Guid id)
    {
        if (await _manager.Artists.RemoveArtistAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("Artist not found");
    }

    public async Task<Result> UpdateArtistAsync(UpdateArtistRequest patch)
    {
        /*
        IMPORTANT: This code works since the MusicDb is SCOPED, but adding one more SaveChanges
        in the same HTML request can cause some problems.
        */
        Artist? artist = await _manager.Artists.GetTrackedArtistAsync(Guid.Parse(patch.Id));

        if (artist == null) return Result.Fail("Artist not found");
        if (!artist.SetName(patch.Name)) return Result.Fail("Empty name provided");
        if (!artist.SetImageUrl(patch.ImageUrl)) return Result.Fail("Wrong image url");

        await _manager.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<ArtistDetailDTO>> GetArtistDetailAsync(Guid id)
    {
        Artist? artist = await _manager.Artists.GetArtistDetailAsync(id);
        if (artist == null)
            return Result<ArtistDetailDTO>.Fail("Artist not found");
        else
            return Result<ArtistDetailDTO>.Ok(ArtistDetailDTO.FromArtist(artist));
    }

    //USER
    public async Task<Result> RemoveUserAsync(Guid id)
    {
        if (await _manager.Users.RemoveUserAsync(id))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("User not found");
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
            return Result<UserDTO>.Fail("User not found");
        else
            return Result<UserDTO>.Ok(UserDTO.FromUser(user));
    }

    public async Task<Result<UserDetailDTO>> GetUserDetailAsync(Guid id)
    {
        User? user = await _manager.Users.GetUserDetailAsync(id);
        if (user == null)
            return Result<UserDetailDTO>.Fail("User not found");
        else
            return Result<UserDetailDTO>.Ok(UserDetailDTO.FromUser(user));
    }
    
    public async Task<Result> CreateUserAsync(string name, string role, string password)
    {
        if (await _manager.Users.CreateUserAsync(name, Enum.Parse<UserRole>(role), password))
        {
            await _manager.SaveChangesAsync();
            return Result.Ok();
        }
        return Result.Fail("User already exists");
        
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