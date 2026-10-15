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
    string[]? Others
);

public record AddAlbumRequest(
    string Title,
    string ArtistName,
    string ImageUrl,
    int ReleaseYear,
    AddTrackRequest[] Tracks
);

public record UpdateTrackInAlbumRequest(
    string Title,
    string ArtistName,
    string[]? Others,
    int NumberInTheAlbum,
    Guid? Id
);

public record UpdateTrackRequest(
    string Title,
    string[]? Others,
    Guid Id
);

public record UpdateAlbumRequest(
    string Title,
    string ArtistName,
    string ImageUrl,
    int ReleaseYear,
    Guid Id,
    UpdateTrackInAlbumRequest[] Tracks
);

public record UpdateArtistRequest(
    string Name,
    string ImageUrl,
    Guid Id
);

public record AddTagOnTrackRequest(
    string TagName,
    Guid TrackId,
    Guid UserId
);

public record AddTagOnAlbumRequest(
    string TagName,
    Guid AlbumId,
    Guid UserId
);
