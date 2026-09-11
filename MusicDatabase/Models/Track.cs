public class Track
{
    public Guid Id {get; private set;}
    public string Title {get; private set;}
    public Guid AlbumId {get; private set;}
    public Album Album {get; private set;}
    public Guid ArtistId {get; private set;}
    public Artist Artist {get; private set;}
    public List<Artist> Others {get; private set;}
    public Genre Genre {get; private set;}

    public Track(string title, Album album, Artist artist, List<Artist> others, Genre genre)
    {
        Id = Guid.NewGuid();
        Title = title;
        AlbumId = album.Id;
        Album = album;
        ArtistId = artist.Id;
        Artist = artist;
        Others = others;
        Genre = genre;
    }

    private Track () {}

    public bool SetTitle(string title)
    {
        if (title.Trim().Length < 1) return false;
        Title = title;
        return true;
    }

    public bool SetAlbum(Album album)
    {
        Album = album;
        AlbumId = album.Id;
        return true;
    }

    public bool SetArtist(Artist artist)
    {
        Artist = artist;
        ArtistId = artist.Id;
        return true;
    }

    public bool SetOthers(List<Artist> others)
    {
        Others = others;
        return true;
    }

    public bool SetGenre(Genre genre)
    {
        Genre = genre;
        return true;
    }
}