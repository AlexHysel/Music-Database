namespace MusicDatabase.Domain;

/// <summary>
/// Represents a user.
/// </summary>
/// <remarks>
/// <b>Rules:</b>
/// <list type="bullet">
/// <item><description>Must have a unique Name (3 to 20 chars).</description></item>
/// <item><description>Name must have no whitespaces.</description></item>
/// <item><description>Must have a Password (8 to 30 chars).</description></item>
/// <item><description>Password must have no whitespaces.</description></item>
/// <item><description>Must have a Role.</description></item>
/// </list>
/// </remarks>
public class User
{
    public Guid Id {get; private set;}
    public string Name {get; private set;}
    public UserRole Role {get; private set;} = UserRole.User;
    public string Password {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    public DateTimeOffset UpdatedAt {get; private set;}

    public User(string name, string password)
    {
        name = name.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length is < 3 or > 20)
            throw new ArgumentOutOfRangeException(nameof(name), "User name must be between 3 and 20 characters.");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
        if (password.Length < 4)
            throw new ArgumentOutOfRangeException(nameof(password), "Password must be > 4 characters.");

        Id = Guid.NewGuid();
        Name = name;
        Password = password;
    }

    public void SetName(string name)
    {
        name = name.Trim();
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (name.Length is < 3 or > 20)
            throw new ArgumentOutOfRangeException(nameof(name), "User name must be between 3 and 20 characters.");
        if (name.ContainsAny(['\n', '\t', ' ']))
            throw new ArgumentOutOfRangeException(nameof(name), "User name must not include spaces.");

        Name = name;
    }

    public static bool ValidateName(string name)
    {
        name = name.Trim();
        if (name.Length is < 3 or > 20)
            return false;
        if (name.ContainsAny(['\n', '\t', ' ']))
            return false;

        return true;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
    }

    public void SetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
        if (password.Length is < 8 or > 30)
            throw new ArgumentOutOfRangeException(nameof(password), "Password must be between 4 and 30 characters.");
        if (password.ContainsAny([' ', '\n', '\t']))
            throw new ArgumentOutOfRangeException(nameof(password), "Password must not include spaces.");

        Password = password;
    }

    public static bool ValidateRawPassword(string password)
    {
        password = password.Trim();
        if (password.Length is < 8 or > 30)
            return false;
        if (password.ContainsAny([' ', '\n', '\t']))
            return false;
        
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