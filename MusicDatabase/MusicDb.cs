
using Microsoft.EntityFrameworkCore;

class MusicDb : DbContext
{
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Playlist> Playlists => Set<Playlist>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=music_db;Username=postgres;Password=6891");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ARTIST
        modelBuilder.Entity<Artist>()
            .HasIndex(a => a.Name)
            .IsUnique();
        
        modelBuilder.Entity<Artist>()
            .HasMany(a => a.Albums)
            .WithOne(a => a.Artist)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // ALBUM
        modelBuilder.Entity<Album>()
            .Property(a => a.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Album>()
            .HasMany(a => a.Tracks)
            .WithOne(t => t.Album)
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        // TRACK
        modelBuilder.Entity<Track>()
            .HasMany(t => t.Artists)
            .WithMany(a => a.Tracks)
            .UsingEntity(j => j.ToTable("TrackArtists"));
        
        modelBuilder.Entity<Track>()
            .Property(t => t.Genre)
            .HasConversion<string>();
        
        // USER
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteTracks)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserTracks"));
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteArtists)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserArtists"));
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteAlbums)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserAlbums"));
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Playlists)
            .WithOne(p => p.Creator)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.SetNull);

        // PLAYLIST
        modelBuilder.Entity<Playlist>()
            .HasMany(p => p.Tracks)
            .WithMany()
            .UsingEntity(j => j.ToTable("PlaylistTracks"));
    }
}