class Logging
{
    public static void Error(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write("[ERROR]");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(" " + message);
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    public static void Success(string message)
    {
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("[SUCCESS]");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" " + message);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void Warning(string message)
    {
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.Write("[WARNING]");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" " + message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}