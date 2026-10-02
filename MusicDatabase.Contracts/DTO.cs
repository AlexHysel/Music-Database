using MusicDatabase.Common;
using MusicDatabase.Data;

namespace MusicDatabase.Contracts;

public record TrackDTO(
    string Title,
    string AlbumTitle,
    string AlbumImageUrl,
    string AlbumId,
    string ArtistName,
    string ArtistId,
    ArtistDTO[] Others,
    string Id)
{
    public static TrackDTO FromTrack(Track track)
    {
        return new TrackDTO(
            track.Title,
            track.Album?.Title ?? "",
            track.Album?.ImageUrl ?? "",
            track.Album?.Id.ToString() ?? "",
            track.Artist?.Name ?? "",
            track.Artist?.Id.ToString() ?? "",
            track.Others.Select(o => ArtistDTO.FromArtist(o)).ToArray(),
            track.Id.ToString()
        );
    }
}

public record TrackDetailDTO(
    string Title,
    string AlbumTitle,
    string AlbumImageUrl,
    string AlbumId,
    string ArtistName,
    string ArtistId,
    ArtistDTO[] Others,
    string Genre,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Id)
{
    public static TrackDetailDTO FromTrack(Track track)
    {
        return new TrackDetailDTO(
            track.Title,
            track.Album?.Title ?? "",
            track.Album?.ImageUrl ?? "",
            track.Album?.Id.ToString() ?? "",
            track.Artist?.Name ?? "",
            track.Artist?.Id.ToString() ?? "",
            track.Others.Select(o => ArtistDTO.FromArtist(o)).ToArray(),
            track.Genre.ToString(),
            track.CreatedAt,
            track.UpdatedAt,
            track.Id.ToString());
    }
}

public record AlbumDTO(
    string Title,
    string ArtistName,
    string ArtistId,
    int ReleaseYear,
    string ImageUrl,
    string Id)
{
    public static AlbumDTO FromAlbum(Album album){
        return new AlbumDTO(
            album.Title,
            album.Artist?.Name ?? "",
            album.Artist?.Id.ToString() ?? "",
            album.ReleaseYear,
            album.ImageUrl,
            album.Id.ToString()
        );
    }
}

public record AlbumDetailDTO(
    string Title,
    string ImageUrl,
    string ArtistName,
    string ArtistId,
    TrackDTO[] Tracks,
    int ReleaseYear,
    string Type,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Id)
{
    public static AlbumDetailDTO FromAlbum(Album album)
    {
        return new AlbumDetailDTO(
            album.Title,
            album.ImageUrl,
            album.Artist?.Name ?? "",
            album.Artist?.Id.ToString() ?? "",
            album.Tracks == null ? new TrackDTO[0] : album.Tracks.Select(t => TrackDTO.FromTrack(t)).ToArray(),
            album.ReleaseYear,
            album.Type.ToString(),
            album.CreatedAt,
            album.UpdatedAt,
            album.Id.ToString());
    }
}

public record ArtistDTO(
    string Name,
    string ImageUrl,
    string Id)
{
    public static ArtistDTO FromArtist(Artist artist)
    {
        return new ArtistDTO(artist.Name, artist.ImageUrl, artist.Id.ToString());
    }
}

public record ArtistDetailDTO(
    string Name,
    string ImageUrl,
    AlbumDTO[] Albums,
    TrackDTO[] Tracks,
    TrackDTO[] AppearsOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Id)
{
    public static ArtistDetailDTO FromArtist(Artist artist)
    {
        var albums = artist.Albums ?? new List<Album>();
        var tracks = artist.Tracks ?? new List<Track>();
        var appearsOn = artist.AppearsOn ?? new List<Track>();
        return new ArtistDetailDTO(
            artist.Name,
            artist.ImageUrl,
            albums.Select(a => AlbumDTO.FromAlbum(a)).ToArray(),
            tracks.Select(t => TrackDTO.FromTrack(t)).ToArray(),
            appearsOn.Select(t => TrackDTO.FromTrack(t)).ToArray(),
            artist.CreatedAt,
            artist.UpdatedAt,
            artist.Id.ToString());
    }
}

public record UserDTO(
    string Name,
    string Role,
    string Id)
{
    public static UserDTO FromUser(User user)
    {
        return new UserDTO(user.Name, user.Role.ToString(), user.Id.ToString());
    }
}

public record UserDetailDTO(
    string Name,
    string Role,
    string Id,
    ArtistDTO[] FavoriteArtists,
    AlbumDTO[] FavoriteAlbums,
    TrackDTO[] FavoriteTracks,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
)
{
    public static UserDetailDTO FromUser(User user)
    {
        return new UserDetailDTO(
            user.Name,
            user.Role.ToString(),
            user.Id.ToString(),
            user.FavoriteArtists.Select(a => ArtistDTO.FromArtist(a)).ToArray(),
            user.FavoriteAlbums.Select(a => AlbumDTO.FromAlbum(a)).ToArray(),
            user.FavoriteTracks.Select(t => TrackDTO.FromTrack(t)).ToArray(),
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}

public record AuthDTO(
    string Token
);

public record SearchResultDTO(
    PagedResult<ArtistDTO> Artists,
    PagedResult<AlbumDTO> Albums,
    PagedResult<TrackDTO> Tracks,
    PagedResult<UserDTO> Users
);