// PRESENTATION LAYER

using System.Linq.Expressions;

class ConsoleUI(Orchestrator orchestrator)
{
    Orchestrator _orchestrator = orchestrator;

    public async Task Run()
    {
        Console.WriteLine("=== MusicDatabase ===");
        string userInput;
        do
        {
            userInput = ReadLine("\n: ");
            string[] parts = userInput.Split(' ', 2);
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
                    int size = Convert.ToInt32(ReadLine("Page size: "));
                    int page = Convert.ToInt32(ReadLine("Page number: "));
                    await DisplayUsersAsync(size, page);
                    break;
                case "removeuser":
                    string name = ReadLine("Name: ");
                    await _orchestrator.RemoveUserAsync(name);
                    break;

                //Track
                case "tracklist":
                    size = Convert.ToInt32(ReadLine("Page size: "));
                    page = Convert.ToInt32(ReadLine("Page number: "));
                    if (parts.Length > 1)
                        await DisplayTracksAsync(size, page, DefineFilter(parts[1]));
                    else
                        await DisplayTracksAsync(size, page, t => true);
                    break;
                case "addtrack":
                    string title = ReadLine("Title: ");
                    string artistName = ReadLine("Main Artist Name: ");
                    string albumTitle = ReadLine("Album Title: ");
                    string[] others = ReadLine("Other Artists Name: ").Split(',');
                    Genre genre;
                    Enum.TryParse(ReadLine("Genre: "), true, out genre);
                    await _orchestrator.AddTrackAsync(title, artistName, others, albumTitle, genre);
                    break;
                case "rmtrack":
                    title = ReadLine("Title: ");
                    albumTitle = ReadLine("Album Title: ");
                    await _orchestrator.RemoveTrackAsync(title, albumTitle);
                    break;
    
                //Album
                case "albumlist":
                    size = Convert.ToInt32(ReadLine("Page size: "));
                    page = Convert.ToInt32(ReadLine("Page number: "));
                    await DisplayAlbumsAsync(size, page, parts);
                    break;
                case "rmalbum":
                    await RmAlbum();
                    break;
                
                //Artist
                case "artistlist":
                    size = Convert.ToInt32(ReadLine("Page size: "));
                    page = Convert.ToInt32(ReadLine("Page number: "));
                    await DisplayArtistsAsync(size, page);
                    break;
                case "rmartist":
                    name = ReadLine("Name: ").Trim();
                    if (string.IsNullOrEmpty(name))
                        Logging.Error("Name can't be empty.");
                    else
                        await _orchestrator.RemoveArtistAsync(name);
                    break;

                default:
                    Logging.Error("Unknown Command");
                    break;
            }
        }
        while (!userInput.Equals("quit"));
    }

    async Task RmAlbum()
    {
        string title = ReadLine("Title: ");

        if (string.IsNullOrEmpty(title))
            Logging.Error("Title can't be empty.");
        else
        {
            string artistName = ReadLine("Artist Name: ");
            if (string.IsNullOrEmpty(artistName))
                Logging.Error("Artist can't be empty");
            else
                await _orchestrator.RemoveAlbumAsync(title, artistName);
        }
    }

    async Task SignIn()
    {
        string name = ReadLine("Name: ");

        if (string.IsNullOrEmpty(name))
            Logging.Error("Name can't be empty.");
        else
        {
            string password = ReadLine("Password: "); 
            if (string.IsNullOrEmpty(password))
                Logging.Error("Password can't be empty.");
            else
                await _orchestrator.AddUserAsync(name, password);       
        }
    }

    async Task DisplayUsersAsync(int size, int page)
    {
        UserDTO[] users = await _orchestrator.GetUsersAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < users.Length; i++)
            Console.WriteLine($"{skipped + i + 1}. {users[i].Name} ({users[i].Id})");
    }

    async Task DisplayAlbumsAsync(int size, int page, string[] parts)
    {

        AlbumDTO[] albums = await _orchestrator.GetAlbumsAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < albums.Length; i++)
            Console.WriteLine($"{skipped + i + 1}. {albums[i].Title} ({albums[i].Type}) by {albums[i].ArtistName}");
    }

    async Task DisplayTracksAsync(int size, int page, Expression<Func<Track, bool>> filter)
    {
        TrackDTO[] tracks = await _orchestrator.GetTracksAsync(size, page, filter);
        int skipped = size * --page;
        for (int i = 0; i < tracks.Length; i++)
        {
            string artists = string.Join(", ", tracks[i].ArtistNames);
            Console.WriteLine($"{skipped + i + 1}. {tracks[i].Title} by {artists}");
        }
    }

    async Task DisplayArtistsAsync(int size, int page)
    {
        ArtistDTO[] artists = await _orchestrator.GetArtistsAsync(size, page);
        int skipped = size * --page;
        for (int i = 0; i < artists.Length; i++)
            Console.WriteLine($"{skipped + i + 1}. {artists[i].Name} ({artists[i].Id})");
    }

    static void DisplayHelp()
    {
        Console.WriteLine("===== COMMANDS =====");
        Console.WriteLine("signin");
        Console.WriteLine("userlist");
        Console.WriteLine("rmuser");
        Console.WriteLine("login");
        Console.WriteLine("albumlist");
        Console.WriteLine("rmalbum");
        Console.WriteLine("tracklist");
        Console.WriteLine("rmtrack");
    }

    static string ReadLine(string text)
    {
        Console.Write(text);
        return Console.ReadLine()?.Trim() ?? "";
    }

    static Expression<Func<Track, bool>> DefineFilter(string filter)
    {
        string[] pair = filter.Split('=');
        pair[0] = pair[0].ToLower();

        if (pair.Length == 2)
            switch (pair[0])
            {
                case "artist":
                    return t => t.Artists.Any(a => a.Name == pair[1]);
                case "genre":
                    return t => t.Genre == Enum.Parse<Genre>(pair[1], true);
            }
        return t => true;
    }
}