using System.Reflection;

namespace MusicDatabase.Data;

internal static class MigrationHelper
{
    public static string Read(string fileName)
    {
        var asm = typeof(MigrationHelper).Assembly;
        var resource = asm.GetManifestResourceNames()
            .Single(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        using var stream = asm.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}