public record TrackDTO(
    string Title,
    string AlbumTitle,
    string ArtistName,
    string[] OthersNames,
    string Genre,
    string Id
);

public record AlbumDTO(
    string Title,
    string ArtistName,
    string[] Tracks,
    string Type,
    string Id
);

public record ArtistDTO(
    string Name,
    string Id
);

public record UserDTO(
    string Name,
    string Role,
    string Id
);

public record AuthDTO(
    string Token
);

public record SearchResultDTO(
    ArtistDTO[] Artists,
    AlbumDTO[] Albums,
    TrackDTO[] Tracks,
    UserDTO[] Users
);