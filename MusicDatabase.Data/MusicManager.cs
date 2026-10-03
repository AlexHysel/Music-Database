using Microsoft.EntityFrameworkCore;
using MusicDatabase.Domain;

namespace MusicDatabase.Data;

// DATA ACCESS LAYER
public class MusicManager: IMusicManager
{
    private readonly MusicDb _context;
    public UserRepository Users {get;}
    public ArtistRepository Artists {get;}
    public AlbumRepository Albums {get;}
    public TrackRepository Tracks {get;}

    public MusicManager(MusicDb context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Artists = new ArtistRepository(_context);
        Albums = new AlbumRepository(_context);
        Tracks = new TrackRepository(_context);
    } 

    async public Task SaveChangesAsync()
    {
        var updatedAlbums = _context.ChangeTracker.Entries<Album>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        foreach (var entry in _context.ChangeTracker.Entries<Track>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdateUpdatedAt();
            else if (entry.State == EntityState.Added)
                entry.Entity.InitializeTimestamps();
        }
        foreach (var entry in _context.ChangeTracker.Entries<Album>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdateUpdatedAt();
            else if (entry.State == EntityState.Added)
                entry.Entity.InitializeTimestamps();
        }
        foreach (var entry in _context.ChangeTracker.Entries<Artist>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.InitializeTimestamps();
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdateUpdatedAt();
        }
        foreach (var entry in _context.ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.InitializeTimestamps();
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdateUpdatedAt();
        }
        await _context.SaveChangesAsync();
    }
}