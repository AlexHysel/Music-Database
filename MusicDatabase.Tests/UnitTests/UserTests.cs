using MusicDatabase.Domain;

namespace MusicDatabase.Tests.UnitTests;

public class UserTests
{
    [Fact]
    public void User_InitializeTimestamps_OnlyOnce()
    {
        var user = new User("user", "password");

        user.InitializeTimestamps();

        Assert.NotEqual(default, user.CreatedAt);
        Assert.NotEqual(default, user.UpdatedAt);
        Assert.Throws<InvalidOperationException>(() => user.InitializeTimestamps());
    }

    [Fact]
    public void User_SetName_TrimAndNullChecksApply()
    {
        var user = new User("user", "password");

        Assert.Throws<ArgumentException>(() => user.SetName("   "));

        user.SetName("  UpdatedUser  ");

        Assert.Equal("UpdatedUser", user.Name);
    }
}