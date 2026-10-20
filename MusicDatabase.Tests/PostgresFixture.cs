using Microsoft.EntityFrameworkCore;
using MusicDatabase.Data;
using Npgsql;
using Testcontainers.PostgreSql;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<Func<MusicDb>> CreateDbFactoryAsync()
    {
        var cs = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}"
        }.ToString();

        var options = new DbContextOptionsBuilder<MusicDb>().UseNpgsql(cs).Options;

        await using (var init = new MusicDb(options))
            await init.Database.MigrateAsync();

        return () => new MusicDb(options);
    }
}

[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>;