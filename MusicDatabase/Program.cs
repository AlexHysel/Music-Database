using (var db = new MusicDb())
{
    db.Database.EnsureCreated();
    string userInput = "";
    do
    {
        userInput = Console.ReadLine().Trim();
        switch (userInput.Split(' ')[0])
        {
            case "help":
                DisplayHelp();
                break;
            case "adduser":
                AddUser();
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

static bool AddUser()
{
    return true;
}