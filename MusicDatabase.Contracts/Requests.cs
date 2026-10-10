namespace MusicDatabase.Contracts;

public record LogInRequest(
    string Username,
    string Password
);

public record SignUpRequest(
    string Username,
    string Password
);

public record AddTrackRequest(
    string Title,
    string Genre,
    string[]? Others
);

public record AddAlbumRequest(
    string Title,
    string ArtistName,
    string ImageUrl,
    int ReleaseYear,
    AddTrackRequest[] Tracks
);

public record UpdateTrackRequest(
    string Title,
    string Genre,
    int NumberInTheAlbum,
    string[]? Others,
    string Id
);

public record UpdateAlbumRequest(
    string Title,
    string ArtistName,
    string ImageUrl,
    int ReleaseYear,
    string Id,
    UpdateTrackRequest[] Tracks
);

public record UpdateArtistRequest(
    string Name,
    string ImageUrl,
    string Id
);
