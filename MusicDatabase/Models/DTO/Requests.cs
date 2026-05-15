public record LogInRequest(
    string Name,
    string Password
);

public record SignUpRequest(
    string Username,
    string Role,
    string Password
);

public record AddTrackRequest(
    string Title,
    string AlbumTitle,
    string Artist,
    string Genre,
    string[]? Others
);
