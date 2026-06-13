using System.Linq;

public record TrackDTO(
    string Title,
    string Id)
{
    public static TrackDTO FromTrack(Track track)
    {
        return new TrackDTO(
            track.Title,
            track.Id.ToString()
        );
    }
}

public record TrackDetailDTO(
    string Title,
    string AlbumTitle,
    string AlbumId,
    string ArtistName,
    string ArtistId,
    string[] OthersNames,
    string[] OthersIds,
    string Genre,
    string Id)
{
    public static TrackDetailDTO FromTrack(Track track)
    {
        return new TrackDetailDTO(
            track.Title,
            track.Album?.Title ?? string.Empty,
            track.Album?.Id.ToString() ?? string.Empty,
            track.Artist?.Name ?? string.Empty,
            track.Artist?.Id.ToString() ?? string.Empty,
            track.Others?.Select(o => o.Name).ToArray() ?? new string[0],
            track.Others?.Select(o => o.Id.ToString()).ToArray() ?? new string[0],
            track.Genre.ToString(),
            track.Id.ToString());
    }
}

public record AlbumDTO(
    string Title,
    string ArtistName,
    string ArtistId,
    string[] TrackTitles,
    string[] TrackIds,
    string Type,
    string Id)
{
    public static AlbumDTO FromAlbum(Album album)
    {
        return new AlbumDTO(
            album.Title,
            album.Artist?.Name ?? string.Empty,
            album.Artist?.Id.ToString() ?? string.Empty,
            album.Tracks?.Select(t => t.Title).ToArray() ?? new string[0],
            album.Tracks?.Select(t => t.Id.ToString()).ToArray() ?? new string[0],
            album.Type.ToString(),
            album.Id.ToString());
    }
}

public record ArtistDTO(
    string Name,
    string Id)
{
    public static ArtistDTO FromArtist(Artist artist)
    {
        return new ArtistDTO(artist.Name, artist.Id.ToString());
    }
}

public record ArtistDetailDTO(
    string Name,
    string[] AlbumsTitles,
    string[] AlbumsIds,
    string[] TracksTitles,
    string[] TracksIds,
    string Id)
{
    public static ArtistDetailDTO FromArtist(Artist artist)
    {
        var albums = artist.Albums ?? new List<Album>();
        var tracks = artist.Tracks ?? new List<Track>();
        return new ArtistDetailDTO(
            artist.Name,
            albums.Select(a => a.Title).ToArray(),
            albums.Select(a => a.Id.ToString()).ToArray(),
            tracks.Select(t => t.Title).ToArray(),
            tracks.Select(t => t.Id.ToString()).ToArray(),
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

public record AuthDTO(
    string Token
);

public record SearchResultDTO(
    ArtistDTO[] Artists,
    AlbumDTO[] Albums,
    TrackDTO[] Tracks,
    UserDTO[] Users
);