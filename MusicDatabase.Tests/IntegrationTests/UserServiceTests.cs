using Microsoft.Extensions.Configuration;
using MusicDatabase.Contracts;
using MusicDatabase.Core.Services;
using MusicDatabase.Data;

namespace MusicDatabase.Tests.IntegrationTests;

[Collection("postgres")]
public class UserServiceTests(PostgresFixture pg)
{
    [Fact]
    public async Task CreateUserAsync_ValidRequest_CreatesUser()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "supersecretkey1234567890ABCDEF1234"
            })
            .Build();

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new UserService(manager, config);

            var result = await service.CreateUserAsync(new SignUpRequest("newuser", "pass123"));
            Assert.True(result.IsSuccess);

            var user = await manager.Users.GetUserAsync("newuser");
            Assert.NotNull(user);
            Assert.Equal("newuser", user!.Name);
        }
    }

    [Fact]
    public async Task CreateUserAsync_InvalidPassword_ReturnsValidationError()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "supersecretkey1234567890ABCDEF1234"
            })
            .Build();

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new UserService(manager, config);

            var result = await service.CreateUserAsync(new SignUpRequest("user123", "bad"));
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.Error!.Type);
        }
    }

    [Fact]
    public async Task LogInAsync_ValidCredentials_ReturnsJwtToken()
    {
        var newDb = await pg.CreateDbFactoryAsync();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "supersecretkey1234567890ABCDEF1234"
            })
            .Build();

        await using (var db = newDb())
        {
            var manager = new MusicManager(db);
            var service = new UserService(manager, config);

            var created = await service.CreateUserAsync(new SignUpRequest("loginuser", "pass123"));
            Assert.True(created.IsSuccess);

            var result = await service.LogInAsync("loginuser", "pass123");
            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Token));
        }
    }
}
