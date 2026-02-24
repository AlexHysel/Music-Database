using (var db = new MusicDb())
{
    if (db.Database.EnsureCreated())
        Logging.Success("Database successfully created");
    else
        Logging.Success("Database successfully found");

    Orchestrator _orchestrator;
    {
        MusicManager manager = new(db);
        _orchestrator = new(manager);
    }

    Console.WriteLine("=== MusicDatabase ===");
    string userInput = "";
    do
    {
        Console.Write(": ");
        userInput = Console.ReadLine().Trim();
        string[] parts = userInput.Split(' ');
        switch (parts[0])
        {
            case "help":
                DisplayHelp();
                break;
            case "signin":
                await manager.AddUserAsync(parts[1], parts[2]);
                break;
            case "tracklist":
                await manager.DisplayTracksAsync();
                break;
            case "addtrack":
                await _orchestrator.AddTrack();
                break;
            case "rmuser":
                await manager.RemoveUserAsync(u => u.Name == parts[1]);
                break;
            case "rmtrack":
                await manager.RemoveTrackAsync(t => t.Title == parts[1]);
                break;
            case "rmalbum":
                await manager.RemoveAlbumAsync(a => a.Title == parts[1]);
                break;
            case "userlist":
                await manager.DisplayUsersAsync();
                break;
            case "albumlist":
                await manager.DisplayAlbumsAsync();
                break;
            case "artistlist":
                await manager.DisplayArtistAsync();
                break;
            case "rmartist":
                await manager.RemoveArtistAsync(a => a.Name == parts[1]);
                break;
            default:
                Console.WriteLine("Unknown Command");
                break;
        }
    }
    while (!userInput.Equals("quit"));
}   

void DisplayHelp()
{
    Console.WriteLine("===== COMMANDS =====");
    Console.WriteLine("signin [username] [password] - add user");
    Console.WriteLine("rmuser [username] - remove user");
    Console.WriteLine("login [username] [password]");
    Console.WriteLine("logout");
    Console.WriteLine("userlist");
    Console.WriteLine("albumlist");
    Console.WriteLine("rmalbum [title]");
    Console.WriteLine("tracklist");
    Console.WriteLine("rmtrack [title]");
}