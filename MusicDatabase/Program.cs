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

    ConsoleUI _consoleUI = new (_orchestrator);
    await _consoleUI.Run();
}