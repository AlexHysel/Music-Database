public record TrackDTO(string Title,
    string AlbumTitle,
    string[] ArtistNames,
    string Genre
);

public record AlbumDTO(
    string Title,
    string ArtistName,
    string Type
);

public record ArtistDTO(
    string Name,
    string Id
);