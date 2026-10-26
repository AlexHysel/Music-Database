using MusicDatabase.Data;
using MusicDatabase.Domain;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class UserRepositoryTests(PostgresFixture pg)
{
    [Fact]
    public async Task AuthenticateUser_ReturnsUser()
    {
        var newDb = await pg.CreateDbFactoryAsync();

        var password = "password";
        var username = "user";

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            await manager.Users.CreateUserAsync(username, UserRole.User, password);
            await manager.SaveChangesAsync();
        }

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);

            var result = await manager.Users.AuthenticateUser(username, password);
            Assert.NotNull(result);
        }
    }
}