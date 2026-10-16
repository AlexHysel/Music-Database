using MusicDatabase.Data;
using MusicDatabase.Contracts;
using MusicDatabase.Domain;
using MusicDatabase.Common;

namespace MusicDatabase.Core.Services;

public class ArtistService
{
    private readonly IMusicManager _manager;

    public ArtistService(MusicManager manager)
    {
        _manager = manager;
    }

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

        User? user = await _manager.Users.GetUserAsync(userId, true);
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
        
        User? user = await _manager.Users.GetUserAsync(userId, true);
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
        Artist? artist = await _manager.Artists.GetArtistAsync(patch.Id, true);

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
}