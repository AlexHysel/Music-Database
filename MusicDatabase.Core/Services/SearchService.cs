using MusicDatabase.Contracts;
using MusicDatabase.Common;

namespace MusicDatabase.Core.Services;

public class SearchService
{
    private readonly ArtistService _artists;
    private readonly AlbumService _albums;
    private readonly TrackService _tracks;
    private readonly UserService _users;

    public SearchService(ArtistService artists, AlbumService albums, TrackService tracks, UserService users)
    {
        _artists = artists;
        _albums = albums;
        _tracks = tracks;
        _users = users;
    }

    public async Task<SearchResultDTO> Search(string title)
    {
        PagedResult<ArtistDTO> artists = await _artists.GetMatchingArtistsAsync(title, 0, 10);
        PagedResult<AlbumDTO> albums = await _albums.GetMatchingAlbumsAsync(title, 0, 10);
        PagedResult<TrackDTO> tracks = await _tracks.GetMatchingTracksAsync(title, 0, 15);
        PagedResult<UserDTO> users = await _users.GetMatchingUsersAsync(title, 0, 10);
        return new SearchResultDTO(artists, albums, tracks, users);
    }
}