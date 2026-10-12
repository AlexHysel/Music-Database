using MusicDatabase.Data.Repositories;

namespace MusicDatabase.Data;

public interface IMusicManager
{
    public UserRepository Users {get;}
    public ArtistRepository Artists {get;}
    public AlbumRepository Albums {get;}
    public TrackRepository Tracks {get;}
    public TagRepository Tags {get;}
    public Task SaveChangesAsync();
}