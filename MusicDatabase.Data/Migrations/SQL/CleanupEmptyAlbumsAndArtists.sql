CREATE OR REPLACE FUNCTION fn_delete_artist_if_orphan(p_artist_id UUID)
RETURNS VOID AS $$
BEGIN
    IF p_artist_id IS NULL THEN RETURN; END IF;

    DELETE FROM "Artists" a
    WHERE a."Id" = p_artist_id
      AND NOT EXISTS (SELECT 1 FROM "Albums" al WHERE al."ArtistId" = a."Id")
      AND NOT EXISTS (SELECT 1 FROM "Tracks" t WHERE t."ArtistId" = a."Id")
      AND NOT EXISTS (SELECT 1 FROM "TrackArtists" ta WHERE ta."OthersId" = a."Id");
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_tracks_cleanup()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD."AlbumId" IS NOT NULL
       AND (TG_OP = 'DELETE' OR OLD."AlbumId" IS DISTINCT FROM NEW."AlbumId") THEN
        DELETE FROM "Albums" al
        WHERE al."Id" = OLD."AlbumId"
          AND NOT EXISTS (SELECT 1 FROM "Tracks" t WHERE t."AlbumId" = al."Id");
    END IF;

    IF TG_OP = 'DELETE' OR OLD."ArtistId" IS DISTINCT FROM NEW."ArtistId" THEN
        PERFORM fn_delete_artist_if_orphan(OLD."ArtistId");
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS track_cleanup_trigger ON "Tracks";
CREATE TRIGGER track_cleanup_trigger
AFTER UPDATE OR DELETE ON "Tracks"
FOR EACH ROW EXECUTE FUNCTION fn_tracks_cleanup();

CREATE OR REPLACE FUNCTION fn_albums_cleanup()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'DELETE' OR OLD."ArtistId" IS DISTINCT FROM NEW."ArtistId" THEN
        PERFORM fn_delete_artist_if_orphan(OLD."ArtistId");
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS album_cleanup_trigger ON "Albums";
CREATE TRIGGER album_cleanup_trigger
AFTER UPDATE OR DELETE ON "Albums"
FOR EACH ROW EXECUTE FUNCTION fn_albums_cleanup();

CREATE OR REPLACE FUNCTION fn_trackartists_cleanup()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'DELETE' OR OLD."OthersId" IS DISTINCT FROM NEW."OthersId" THEN
        PERFORM fn_delete_artist_if_orphan(OLD."OthersId");
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trackartists_cleanup_trigger ON "TrackArtists";
CREATE TRIGGER trackartists_cleanup_trigger
AFTER UPDATE OR DELETE ON "TrackArtists"
FOR EACH ROW EXECUTE FUNCTION fn_trackartists_cleanup();

DROP FUNCTION IF EXISTS fn_cleanup_empty_album_artist();