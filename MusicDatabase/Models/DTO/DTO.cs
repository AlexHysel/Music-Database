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
    AlbumDTO Album,
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
            AlbumDTO.FromAlbum(track.Album),
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
    string Id)
{
    public static AlbumDTO FromAlbum(Album album){
        return new AlbumDTO(
            album.Title,
            album.Id.ToString()
        );
    }
}

public record AlbumDetailDTO(
    string Title,
    string ArtistName,
    string ArtistId,
    TrackDTO[] Tracks,
    string Type,
    string Id)
{
    public static AlbumDetailDTO FromAlbum(Album album)
    {
        return new AlbumDetailDTO(
            album.Title,
            album.Artist?.Name ?? string.Empty,
            album.Artist?.Id.ToString() ?? string.Empty,
            album.Tracks == null ? new TrackDTO[0] : album.Tracks.Select(t => TrackDTO.FromTrack(t)).ToArray(),
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
    AlbumDTO[] Albums,
    TrackDTO[] Tracks,
    string Id)
{
    public static ArtistDetailDTO FromArtist(Artist artist)
    {
        var albums = artist.Albums ?? new List<Album>();
        var tracks = artist.Tracks ?? new List<Track>();
        return new ArtistDetailDTO(
            artist.Name,
            albums.Select(a => AlbumDTO.FromAlbum(a)).ToArray(),
            tracks.Select(t => TrackDTO.FromTrack(t)).ToArray(),
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