using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MusicDatabase.Domain;

namespace MusicDatabase.Data;

public class MusicDb : DbContext
{
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TrackTagStats> TrackTagStats => Set<TrackTagStats>();
    public DbSet<AlbumTagStats> AlbumTagStats => Set<AlbumTagStats>();

    public MusicDb(DbContextOptions<MusicDb> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ARTIST
        modelBuilder.Entity<Artist>()
            .HasIndex(a => a.Name)
            .IsUnique();

        modelBuilder.Entity<Artist>()
            .Property(a => a.Id)
            .ValueGeneratedNever();
        
        modelBuilder.Entity<Artist>()
            .HasMany(a => a.Albums)
            .WithOne(a => a.Artist)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // ALBUM
        modelBuilder.Entity<Album>()
            .Property(a => a.Type)
            .HasConversion<string>()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        
        modelBuilder.Entity<Album>()
            .Property(a => a.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Album>()
            .HasMany(a => a.Tracks)
            .WithOne(t => t.Album)
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Album>()
            .HasMany(t => t.TagsStats)
            .WithOne(t => t.Album);

        // TRACK
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Artist)
            .WithMany(a => a.Tracks);

        modelBuilder.Entity<Track>()
            .HasIndex(t => new { t.AlbumId, t.NumberInTheAlbum })
            .IsUnique();

        modelBuilder.Entity<Track>()
            .Property(t => t.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Track>()
            .HasMany(t => t.Others)
            .WithMany(a => a.AppearsOn)
            .UsingEntity(j => j.ToTable("TrackArtists"));
        
        modelBuilder.Entity<Track>()
            .HasMany(t => t.TagsStats)
            .WithOne(t => t.Track);
        
        // USER
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
        
        // TAGS
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name);

        // TRACK TAG VOTE
        modelBuilder.Entity<TrackTagStats>()
            .HasMany(t => t.VotedBy)
            .WithMany();
        
        modelBuilder.Entity<TrackTagStats>()
            .HasOne(t => t.Tag)
            .WithMany()
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TrackTagStats>()
            .HasOne(t => t.Track)
            .WithMany(t => t.TagsStats)
            .HasForeignKey(t => t.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        // ALBUM TAG VOTE
        modelBuilder.Entity<AlbumTagStats>()
            .HasMany(t => t.VotedBy)
            .WithMany();
        
        modelBuilder.Entity<AlbumTagStats>()
            .HasOne(t => t.Tag)
            .WithMany()
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AlbumTagStats>()
            .HasOne(t => t.Album)
            .WithMany(t => t.TagsStats)
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}