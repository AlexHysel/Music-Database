DROP FUNCTION IF EXISTS fn_update_album_type();

CREATE OR REPLACE FUNCTION fn_update_album_type()
RETURNS TRIGGER AS $$
DECLARE
    album_id UUID;
    track_count INT;
BEGIN
    album_id := COALESCE(NEW."AlbumId", OLD."AlbumId");

    SELECT COUNT(*) INTO track_count 
    FROM "Tracks"
    WHERE "AlbumId" = album_id;

    UPDATE "Albums"
    SET "Type" = CASE
        WHEN track_count = 1 THEN 'Single'
        WHEN track_count BETWEEN 2 AND 5 THEN 'EP'
        ELSE 'Album'
    END
    WHERE "Id" = album_id;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;