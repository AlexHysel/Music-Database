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
    public DbSet<TrackTagVote> TrackTagVotes => Set<TrackTagVote>();
    public DbSet<AlbumTagVote> AlbumTagVotes => Set<AlbumTagVote>();

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

        // TRACK
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Artist)
            .WithMany(a => a.Tracks);

        // Makes the number of the track unique within the album
        //modelBuilder.Entity<Track>()
        //    .HasIndex(t => new { t.AlbumId, t.NumberInTheAlbum })
        //    .IsUnique();

        modelBuilder.Entity<Track>()
            .Property(t => t.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Track>()
            .HasMany(t => t.Others)
            .WithMany(a => a.AppearsOn)
            .UsingEntity(j => j.ToTable("TrackArtists"));
        
        modelBuilder.Entity<Track>()
            .HasMany(t => t.Tags)
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
        
        // TAGS
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name);

        // TRACK TAG VOTE
        modelBuilder.Entity<TrackTagVote>()
            .HasOne(t => t.TaggedBy)
            .WithMany()
            .HasForeignKey(t => t.TaggedById)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TrackTagVote>()
            .HasOne(t => t.Tag)
            .WithMany()
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TrackTagVote>()
            .HasOne(t => t.Track)
            .WithMany(t => t.Tags)
            .HasForeignKey(t => t.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        // ALBUM TAG VOTE
        modelBuilder.Entity<AlbumTagVote>()
            .HasOne(t => t.TaggedBy)
            .WithMany()
            .HasForeignKey(t => t.TaggedById)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AlbumTagVote>()
            .HasOne(t => t.Tag)
            .WithMany()
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AlbumTagVote>()
            .HasOne(t => t.Album)
            .WithMany(t => t.Tags)
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}