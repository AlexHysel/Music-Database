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
                await SignIn();
                break;
            case "userlist":
                Console.Write("Page Size: ");
                int size = Convert.ToInt32(Console.ReadLine().Trim());
                Console.Write("Page Number: ");
                int page = Convert.ToInt32(Console.ReadLine().Trim());

                await DisplayUsersAsync(size, page);
                break;
            case "removeuser":
                Console.Write("Name: ");
                string name = Console.ReadLine().Trim();
                await _orchestrator.RemoveUserAsync(name);
                break;

            //Track
            case "tracklist":
                Console.Write("Page size: ");
                size = Convert.ToInt32(Console.ReadLine().Trim());
                Console.Write("Page number: ");
                page = Convert.ToInt32(Console.ReadLine().Trim());

                await DisplayTracksAsync(size, page);
                break;
            case "addtrack":
                string title = ReadRequiredString("Title: ");
                string artistName = ReadRequiredString("Main Artist Name: ");
                string albumTitle = ReadRequiredString("Album Title: ");
                Console.Write("Other Artists Name: ");
                string[] others = Console.ReadLine().Trim().Split(',');
                Console.Write("Genre: ");
                Genre genre;
                Enum.TryParse(Console.ReadLine(), true, out genre);
                await _orchestrator.AddTrackAsync(title, artistName, others, albumTitle, genre);
                break;
            case "rmtrack":
                Console.Write("Title: ");
                title = Console.ReadLine().Trim();
                Console.Write("Album Title: ");
                albumTitle = Console.ReadLine().Trim();
                await _orchestrator.RemoveTrackAsync(title, albumTitle);
                break;
 
            //Album
            case "albumlist":
                Console.Write("Page size: ");
                size = Convert.ToInt32(Console.ReadLine().Trim());
                Console.Write("Page number: ");
                page = Convert.ToInt32(Console.ReadLine().Trim());

                await DisplayAlbumsAsync(size, page);
                break;
            case "rmalbum":
                await RmAlbum();
                break;
            
            //Artist
            case "artistlist":
                Console.Write("Page size: ");
                size = Convert.ToInt32(Console.ReadLine().Trim());
                Console.Write("Page number: ");
                page = Convert.ToInt32(Console.ReadLine().Trim());
            
                await DisplayArtistsAsync(size, page);
                break;
            case "rmartist":
                Console.Write("Name: ");
                name = Console.ReadLine().Trim();
                await _orchestrator.RemoveArtistAsync(name);
                break;

            default:
                Logging.Error("Unknown Command");
                break;
        }
    }
    while (!userInput.Equals("quit"));

    async Task RmAlbum()
    {
        Console.Write("Title: ");
        string title = Console.ReadLine().Trim();
        if (string.IsNullOrEmpty(title))
            Logging.Error("Title can't be empty.");
        else
        {
            Console.Write("Artist: ");
            string artistName = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(artistName))
                Logging.Error("Artist can't be empty");
            else
                await _orchestrator.RemoveAlbumAsync(title, artistName);
        }
    }

    async Task SignIn()
    {
        Console.Write("Name: ");
        string name = Console.ReadLine().Trim();
        Console.Write("Password: ");
        string password = Console.ReadLine().Trim();
        await _orchestrator.AddUserAsync(name, password);       
    }

    async Task DisplayUsersAsync(int size, int page)
    {
        UserDTO[] users = await _orchestrator.GetUsersAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < users.Length; i++)
            Console.WriteLine($"{skipped + i + 1}. {users[i].Name} ({users[i].Id})");
    }

    async Task DisplayAlbumsAsync(int size, int page)
    {
        AlbumDTO[] albums = await _orchestrator.GetAlbumsAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < albums.Length; i++)
            Console.WriteLine($"{skipped + i + 1}. {albums[i].Title} ({albums[i].Type}) by {albums[i].ArtistName}");
    }

    async Task DisplayTracksAsync(int size, int page)
    {
        TrackDTO[] tracks = await _orchestrator.GetTracksAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < tracks.Length; i++)
        {
            string artists = "";
            foreach (string artist in tracks[i].ArtistNames)
                artists += " " + artist;
            Console.WriteLine($"{skipped + i + 1}. {tracks[i].Title} by{artists}");
        }
    }

    async Task DisplayArtistsAsync(int size, int page)
    {
        ArtistDTO[] artists = await _orchestrator.GetArtistsAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < artists.Length; i++)
            Console.WriteLine($"{skipped + i + 1}. {artists[i].Name} ({artists[i].Id})");
    }
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

string ReadRequiredString(string prompt)
{
    Console.Write(prompt);
    while (true)
    {
        string? answer = Console.ReadLine();
        if (string.IsNullOrEmpty(answer))
            Logging.Error("Invalid value.");
        else
            return answer.Trim();
    }
}