DROP FUNCTION IF EXISTS fn_cleanup_empty_album_artist();

CREATE OR REPLACE FUNCTION fn_cleanup_empty_album_artist()
RETURNS TRIGGER AS $$
DECLARE
    album_id UUID;
BEGIN
    album_id := COALESCE(NEW."AlbumId", OLD."AlbumId");

    IF album_id IS NOT NULL THEN
        IF (SELECT COUNT(*) FROM "Tracks" WHERE "AlbumId" = album_id) = 0 THEN
            DELETE FROM "Albums" WHERE "Id" = album_id;

                        DELETE FROM "Artists" a
                        WHERE NOT EXISTS (SELECT 1 FROM "Albums" al WHERE al."ArtistId" = a."Id")
                            AND NOT EXISTS (SELECT 1 FROM "Tracks" t WHERE t."ArtistId" = a."Id")
                            -- TrackArtists columns were renamed to OthersId/TrackId in migrations; check OthersId
                            AND NOT EXISTS (SELECT 1 FROM "TrackArtists" ta WHERE ta."OthersId" = a."Id");
        END IF;
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS track_cleanup_trigger ON "Tracks";
CREATE TRIGGER track_cleanup_trigger
AFTER INSERT OR UPDATE OR DELETE ON "Tracks"
FOR EACH ROW
EXECUTE FUNCTION fn_cleanup_empty_album_artist();
