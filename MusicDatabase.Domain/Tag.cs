public class Tag
{
    public Guid Id {get; private set;}
    public string Name {get; private set;}

    private Tag() {}

    public Tag(string name)
    {
        name = name.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 20) throw new ArgumentException("Name of the tag is > 20 chars.");

        Id = Guid.NewGuid();
        Name = name.ToLower();
    }
}