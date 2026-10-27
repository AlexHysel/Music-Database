namespace MusicDatabase.Domain;

/// <summary>
/// Represents a user.
/// </summary>
/// <remarks>
/// <b>Rules:</b>
/// <list type="bullet">
/// <item><description>Must have a unique Name (3 to 20 chars).</description></item>
/// <item><description>Must have a Password.</description></item>
/// <item><description>Must have a Role.</description></item>
/// </list>
/// </remarks>
public class User
{
    public Guid Id {get; private set;}
    public string Name {get; private set;}
    public UserRole Role {get; private set;}
    public string Password {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public User(string name, string password, UserRole role)
    {
        name = name.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.NewGuid();
        Name = name;
        Role = role;
        Password = password;
    }

    public bool SetName(string name)
    {
        name = name.Trim();
        if (name.Length < 1) return false;

        Name = name;
        return true;
    }

    public bool SetRole(UserRole role)
    {
        Role = role;
        return true;
    }

    public bool SetPassword(string password)
    {
        Password = password;
        return true;
    }

    public void InitializeTimestamps()
    {
        if (CreatedAt == default)
        {
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
            throw new InvalidOperationException("CreatedAt has already been initialized.");
    }

    public void UpdateUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}