using Microsoft.EntityFrameworkCore;

using (var db = new MusicDb())
{
    db.Database.EnsureCreated();
    MusicManager manager = new(db);
    string userInput = "";
    do
    {
        userInput = Console.ReadLine().Trim();
        string[] parts = userInput.Split(' ');
        switch (parts[0])
        {
            case "help":
                DisplayHelp();
                break;
            case "signin":
                await AddUser(parts[1], parts[2], db);
                break;
            case "tracklist":
                await manager.DisplayTracksAsync();
                break;
            case "addtrack":
                Console.Write("Title: ");
                string title = Console.ReadLine().Trim();

                Console.Write("Artists Name: ");
                string artistName = Console.ReadLine().Trim();
                List<Artist> artists = new();
                foreach (string name in artistName.Split(','))
                {
                    Artist? artist = db.Artists.FirstOrDefault(x => x.Name == name);
                    if (artist == null)
                    {
                        artist = new Artist() {Name = name};
                        Console.WriteLine($"Artist {name} was added.");
                        await db.Artists.AddAsync(artist);
                    }
                    artists.Add(artist);
                }

                Console.Write("Album Title: ");
                string albumTitle = Console.ReadLine().Trim();
                Album? album = await db.Albums.FirstOrDefaultAsync(x => x.Title == albumTitle);
                if (album == null)
                {
                    string name;
                    Console.Write("Album Artist: ");
                    do
                    {
                        name = Console.ReadLine().Trim();
                    }
                    while (!artistName.Split(',').Contains(name) || name == "");
                    album = new() {Artist = artists.First(x => x.Name == name), Type = AlbumType.Single};
                    Console.WriteLine($"Album {album.Title} was added");
                }
                
                Track track = new() {Title = title, Album = album, Artists = artists, Genre = Genre.DARKSYNTH};
                await manager.AddTrackAsync(track);
                break;
        }
    }
    while (!userInput.Equals("quit"));
}   

static void DisplayHelp()
{
    Console.WriteLine("===== COMMANDS =====");
    Console.WriteLine("signin [username] [password] - add user");
    Console.WriteLine("rmuser [username] - remove user");
    Console.WriteLine("login [username] [password]");
    Console.WriteLine("logout");
}

static async Task AddUser(string name, string password, MusicDb db)
{
    User user = new() {Name = name, Password = password};
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
}