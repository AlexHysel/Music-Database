// PRESENTATION LAYER

using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
                case "find":
                    if (parts.Count() == 2)
                        await Find(parts[1]);
                    else
                        Logging.Error("Wrong syntax.");
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
                    await DisplayTracksAsync(size, page, TrackFilter(parts[1]));
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
                    await DisplayAlbumsAsync(size, page, AlbumFilter(parts[1]));
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

    async Task DisplayAlbumsAsync(int size, int page, Expression<Func<Album, bool>> filter)
    {

        AlbumDTO[] albums = await _orchestrator.GetAlbumsAsync(size, page, filter);
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

    static Expression<Func<Track, bool>> TrackFilter(string filter)
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
            Logging.Warning("Wrong filter.");
        return t => true;
    }

    static Expression<Func<Album, bool>> AlbumFilter(string filter)
    {
        string[] pair = filter.Split('=');
        pair[0] = pair[0].ToLower();

        if (pair.Length == 2)
            switch (pair[0])
            {
                case "artist":
                    return a => a.Artist.Name == pair[1];
            }
            Logging.Warning("Wrong filter.");
        return t => true;
    }

    async Task Find(string search)
    {
        search = $"%{search}%";
        ArtistDTO[] artists = await _orchestrator.GetArtistsAsync(a => EF.Functions.ILike(a.Name, search));
        AlbumDTO[] albums = await _orchestrator.GetAlbumsAsync(a => EF.Functions.ILike(a.Title, search));
        TrackDTO[] tracks = await _orchestrator.GetTracksAsync(a => EF.Functions.ILike(a.Title, search));
        UserDTO[] users = await _orchestrator.GetUsersAsync(a => EF.Functions.ILike(a.Name, search));
        
        Console.WriteLine("=== Results ===");
        Console.WriteLine($"- ({artists.Count()}) Artists:");
        foreach (var artist in artists)
            Console.WriteLine(artist.Name);
        Console.WriteLine($"\n- ({albums.Count()}) Albums:");
        foreach (var album in albums)
            Console.WriteLine($"{album.Title} by {album.ArtistName}");
        Console.WriteLine($"\n- ({tracks.Count()}) Tracks:");
        foreach (var track in tracks)
            Console.WriteLine($"{track.Title} from {track.AlbumTitle}");
        Console.WriteLine($"\n- ({users.Count()}) Users:");
        foreach (var user in users)
            Console.WriteLine(user.Name);
    }
}