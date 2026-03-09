// BUSINESS LOGIC LAYER
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

class Orchestrator
{
    private readonly MusicManager _manager;

    public Orchestrator(MusicManager manager) => _manager = manager;

    //TRACK
    public async Task RemoveTrackAsync(string title, string albumTitle)
    {
        await _manager.RemoveTrackAsync(t => t.Album.Title == albumTitle && t.Title == title);
        await _manager.SaveChangesAsync();
    }
    
    public async Task<TrackDTO[]> GetTracksAsync(int size, int page, Expression<Func<Track, bool>> filter)
    {
        IQueryable<TrackDTO> request = _manager.GetTracks(filter);
        return await request.Skip((page - 1) * size).Take(size).ToArrayAsync();
    }

    public async Task<TrackDTO[]> GetTracksAsync(Expression<Func<Track, bool>> filter)
    {
        return await _manager.GetTracks(filter).ToArrayAsync();
    }

    public async Task<TrackDTO[]> GetTracksAsync(int size, int page)
    {
        IQueryable<TrackDTO> request = _manager.GetTracks();
        return await request.Skip((page - 1) * size).Take(size).ToArrayAsync();
    }

    public async Task AddTrackAsync(string title, string artistName, string[] others, string albumTitle, Genre genre)
    {
        Album album = await _manager.EnsureAlbumCreated(albumTitle, await _manager.EnsureArtistCreated(artistName));

        List<Artist> artists = [await _manager.EnsureArtistCreated(artistName)];
        foreach (string name in others)
            if (!string.IsNullOrEmpty(name))
                artists.Add(await _manager.EnsureArtistCreated(name));

        Track track = new() {Title = title, Album = album, Artists = artists, Genre = genre};
        await _manager.AddTrackAsync(track);
        await _manager.SaveChangesAsync();
    }

    //ALBUM
    public async Task RemoveAlbumAsync(string title, string artistName)
    {
        await _manager.RemoveAlbumAsync(a => a.Artist.Name == artistName && a.Title == title);
        await _manager.SaveChangesAsync();
    }

    public async Task<AlbumDTO[]> GetAlbumsAsync(int size, int page, Expression<Func<Album, bool>> filter)
    {
        var request = _manager.GetAlbums(filter);
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public async Task<AlbumDTO[]> GetAlbumsAsync(int size, int page)
    {
        var request = _manager.GetAlbums();
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }
    
    public async Task<AlbumDTO[]> GetAlbumsAsync(Expression<Func<Album, bool>> filter)
    {
        return await _manager.GetAlbums(filter).ToArrayAsync();
    }

    //ARTIST
    public async Task RemoveArtistAsync(string name)
    {
        await _manager.RemoveArtistAsync(a => a.Name == name);
        await _manager.SaveChangesAsync();
    }

    public async Task<ArtistDTO[]> GetArtistsAsync(int size, int page)
    {
        var request = _manager.GetArtists();
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public async Task<ArtistDTO[]> GetArtistsAsync(int size, int page, Expression<Func<Artist, bool>> filter)
    {
        var request = _manager.GetArtists(filter);
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public async Task<ArtistDTO[]> GetArtistsAsync(Expression<Func<Artist, bool>> filter)
    {
        return await _manager.GetArtists(filter).ToArrayAsync();
    }

    //USER
    public async Task RemoveUserAsync(string name)
    {
        await _manager.RemoveUserAsync(u => u.Name == name);
        await _manager.SaveChangesAsync();
    }

    public async Task<UserDTO[]> GetUsersAsync(int size, int page)
    {
        var request = _manager.GetUsers();
        return await request.Skip(size * (page - 1)).Take(size).ToArrayAsync();
    }

    public async Task<UserDTO[]> GetUsersAsync(Expression<Func<User, bool>> filter)
    {
        return await _manager.GetUsers(filter).ToArrayAsync();
    }

    public async Task AddUserAsync(string name, string password)
    {
        await _manager.AddUserAsync(name, password);
    }

    public async Task<UserDTO> LogInAsync(string name, string password)
    {
        if (await _manager.AuthenticateUser(name, password))
        {
            UserDTO? user = await _manager.GetUserAsync(name);
            if (user != null)
                return user;
        }
        return new UserDTO("", "");
    }
}