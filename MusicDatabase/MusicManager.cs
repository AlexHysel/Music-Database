using Microsoft.EntityFrameworkCore;

// DATA ACCESS LAYER
public class MusicManager
{
    private readonly MusicDb _context;

    public MusicManager(MusicDb context) => _context = context;

    async public Task SaveChangesAsync()
    {
        var updatedAlbums = _context.ChangeTracker.Entries<Album>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        await _context.SaveChangesAsync();
    }

    //TRACK
    //  GET
    async public Task<List<Track>> GetMatchingTracksAsync(string title){
        return await _context.Tracks.AsNoTracking().Where(t => t.Title.Contains(title)).Include(t => t.Album).ToListAsync();
    }

    async public Task<Track?> GetTrackAsync(Guid id)
    {
        return await _context.Tracks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackedTrackAsync(Guid id)
    {
        return await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackDetailAsync(Guid id){
        return await _context.Tracks.AsNoTracking()
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    async public Task<Track?> GetTrackedTrackDetailAsync(Guid id){
        return await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.Artist)
            .Include(t => t.Others)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    //  ADD
    async public Task AddTrackAsync(Track track)
    {
        await _context.Tracks.AddAsync(track);
    }

    //  REMOVE
    async public Task<bool> RemoveTrackAsync(Guid id)
    {
        Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id);
        if (track != null)
        {
            _context.Tracks.Remove(track);
            return true;
        }
        return false;
    }

    async public Task<bool> RemoveTrackFromFavoritesAsync(Guid userId, Guid trackId)
    {
        User? user = await _context.Users
            .Include(u => u.FavoriteTracks)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId);
        if (track == null) return false;

        return user.RemoveTrackFromFavorites(track);
    }

    async public Task<bool> AddTrackToFavoritesAsync(Guid userId, Guid trackId)
    {
        User? user = await _context.Users
            .Include(u => u.FavoriteTracks)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        Track? track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId);
        if (track == null) return false;

        return user.AddTrackToFavorites(track);
    }

    // USER
    public async Task<List<User>> GetMatchingUsersAsync(string name){
        return await _context.Users.Where(u => u.Name.Contains(name)).ToListAsync();
    }

    public async Task<User?> GetUserAsync(Guid id){
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetTrackedUserAsync(Guid id){
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserDetailAsync(Guid id){
        return await _context.Users.AsNoTracking()
            .Include(u => u.FavoriteArtists)
            .Include(u => u.FavoriteAlbums)
            .Include(u => u.FavoriteTracks)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<User?> GetTrackedUserDetailAsync(Guid id){
        return await _context.Users
            .Include(u => u.FavoriteArtists)
            .Include(u => u.FavoriteAlbums)
            .Include(u => u.FavoriteTracks)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    async public Task<bool> RemoveUserAsync(Guid id){
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            _context.Users.Remove(user);
            return true;
        }
        return false;
    }

    async public Task<User?> AuthenticateUser(string name, string password)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Name == name);
        if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
            return user;
        else
            return null;
    }

    async public Task<bool> UserExistsAsync(string name)
    {
        return await _context.Users.AnyAsync(u => u.Name == name);
    }

    async public Task<bool> AddUserAsync(string name, UserRole role, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Name == name))
            return false;
        
        User user = new(name, BCrypt.Net.BCrypt.EnhancedHashPassword(password), role);
        await _context.Users.AddAsync(user);
        return true;
    }

    //ALBUM
    //  GET
    async public Task<List<Album>> GetMatchingAlbumsAsync(string title)
    {
        return await _context.Albums.Where(a => a.Title.Contains(title)).ToListAsync();
    }

    async public Task<bool> RemoveAlbumFromFavoritesAsync(Guid userId, Guid albumId)
    {
        User? user = await _context.Users
            .Include(u => u.FavoriteAlbums)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == albumId);
        if (album == null) return false;

        return user.RemoveAlbumFromFavorites(album);
    }

    async public Task<bool> AddAlbumToFavoritesAsync(Guid userId, Guid albumId)
    {
        User? user = await _context.Users
            .Include(u => u.FavoriteAlbums)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == albumId);
        if (album == null) return false;

        return user.AddAlbumToFavorites(album);
    }

    async public Task<bool> RemoveAlbumAsync(Guid id)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == id);
        if (album != null)
        {
            _context.Albums.Remove(album);
            return true;
        }
        return false;
    }

    async public Task<Album?> GetTrackedAlbumAsync(Guid id)
    {
        return await _context.Albums.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Album?> GetAlbumAsync(Guid id)
    {
        return await _context.Albums.AsNoTracking().Include(a => a.Artist).Include(a => a.Tracks).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Album> EnsureAlbumCreated(string title, Artist artist)
    {
        Album? album = await _context.Albums.FirstOrDefaultAsync(a => a.Artist.Id == artist.Id && a.Title == title);
        if (album == null)
        {
            album = new(title, artist);
            await _context.Albums.AddAsync(album);
        }
        return album;
    }

    //ARTIST
    //  GET
    public async Task<List<Artist>> GetMatchingArtistsAsync(string name){
        return await _context.Artists.Where(a => a.Name.Contains(name)).ToListAsync();
    }

    public async Task<Artist?> GetArtistAsync(Guid id){
        return await _context.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artist?> GetTrackedArtistAsync(Guid id){
        return await _context.Artists.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artist?> GetArtistDetailAsync(Guid id){
        return await _context.Artists.AsNoTracking()
            .Include(a => a.Albums)
            .Include(a => a.Tracks)
            .ThenInclude(t => t.Album)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Artist?> GetTrackedArtistDetailAsync(Guid id){
        return await _context.Artists
            .Include(a => a.Albums)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> RemoveArtistFromFavoritesAsync(Guid userId, Guid artistId){
        User? user = await _context.Users
            .Include(u => u.FavoriteArtists)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artistId);
        if (artist == null) return false;

        return user.RemoveArtistFromFavorites(artist);
    }

    public async Task<bool> AddArtistToFavoritesAsync(Guid userId, Guid artistId){
        User? user = await _context.Users
            .Include(u => u.FavoriteArtists)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artistId);
        if (artist == null) return false;

        return user.AddArtistToFavorites(artist);
    }

    public async Task<Artist> EnsureArtistCreated(string name)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name);
        if (artist == null)
            artist = _context.Artists.Local.FirstOrDefault(a => a.Name == name);
        if (artist == null)
        {
            artist = new Artist(name);
            await _context.Artists.AddAsync(artist);
        }
        return artist;
    }

    async public Task<bool> RemoveArtistAsync(Guid id)
    {
        Artist? artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == id);

        if (artist != null)
        {
            _context.Artists.Remove(artist);
            return true;
        }
        return false;
    }
}