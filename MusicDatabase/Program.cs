// PRESENTATION LAYER
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

            //User
            case "signin":
                await _orchestrator.AddUserAsync();
                break;
            case "userlist":
                await _orchestrator.DisplayUsersAsync();
                break;
            case "removeuser":
                await _orchestrator.RemoveUserAsync();
                break;

            //Track
            case "tracklist":
                await _orchestrator.DisplayTracksAsync();
                break;
            case "addtrack":
                await _orchestrator.AddTrackAsync();
                break;
            case "rmtrack":
                await _orchestrator.RemoveTrackAsync();
                break;
                
            //Album
            case "albumlist":
                await _orchestrator.DisplayAlbumsAsync();
                break;
            case "rmalbum":
                await _orchestrator.RemoveAlbumAsync();
                break;
            
            //Artist
            case "artistlist":
                await _orchestrator.DisplayArtistsAsync();
                break;
            case "rmartist":
                await _orchestrator.RemoveArtistAsync();
                break;

            default:
                Logging.Error("Unknown Command");
                break;
        }
    }
    while (!userInput.Equals("quit"));
}   

void DisplayHelp()
{
    Console.WriteLine("===== COMMANDS =====");
    Console.WriteLine("signin");
    Console.WriteLine("userlist");
    Console.WriteLine("rmuser");
    Console.WriteLine("login [username] [password]");
    Console.WriteLine("albumlist");
    Console.WriteLine("rmalbum [title]");
    Console.WriteLine("tracklist");
    Console.WriteLine("rmtrack [title]");
}