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

    [Fact]
    public void User_ValidateRawPassword_RejectsInvalidFormats()
    {
        Assert.True(User.ValidateRawPassword("pass1234"));
        Assert.False(User.ValidateRawPassword("pass"));
        Assert.False(User.ValidateRawPassword("p"));
        Assert.False(User.ValidateRawPassword("pass 1234"));
        Assert.False(User.ValidateRawPassword("pass\n1234"));
        Assert.False(User.ValidateRawPassword("pass\t1234"));
        Assert.False(User.ValidateRawPassword(new string('a', 31)));
    }

    [Fact]
    public void User_Constructor_RejectsPasswordsWithWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new User("user", " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => new User("user", "pass 1234"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new User("user", "pass\n1234"));
    }
}