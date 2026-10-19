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

    public async Task<PagedResult<ArtistDTO>> GetMatchingArtistsAsync(string name, int toSkip, int toTake)
    {
        return (await _manager.Artists.GetMatchingArtistsAsync(name, toSkip, toTake))
            .Map(a => ArtistDTO.FromArtist(a));
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
        in the same request can cause some problems.
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