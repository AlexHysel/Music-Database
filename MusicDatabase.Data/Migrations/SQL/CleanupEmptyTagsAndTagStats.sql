CREATE OR REPLACE FUNCTION fn_delete_tag_if_orphan(p_tag_id UUID)
RETURNS VOID AS $$
BEGIN
    IF p_tag_id IS NULL THEN RETURN; END IF;

    DELETE FROM "Tags" t
    WHERE t."Id" = p_tag_id
      AND NOT EXISTS (SELECT 1 FROM "TrackTagStats" tts WHERE tts."TagId" = t."Id")
      AND NOT EXISTS (SELECT 1 FROM "AlbumTagStats" ats WHERE ats."TagId" = t."Id");
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_tracktagstats_delete_cleanup()
RETURNS TRIGGER AS $$
BEGIN
    PERFORM fn_delete_tag_if_orphan(OLD."TagId");
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tracktagstats_delete_cleanup_trigger ON "TrackTagStats";
CREATE TRIGGER tracktagstats_delete_cleanup_trigger
AFTER DELETE ON "TrackTagStats"
FOR EACH ROW EXECUTE FUNCTION fn_tracktagstats_delete_cleanup();

CREATE OR REPLACE FUNCTION fn_albumtagstats_delete_cleanup()
RETURNS TRIGGER AS $$
BEGIN
    PERFORM fn_delete_tag_if_orphan(OLD."TagId");
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS albumtagstats_delete_cleanup_trigger ON "AlbumTagStats";
CREATE TRIGGER albumtagstats_delete_cleanup_trigger
AFTER DELETE ON "AlbumTagStats"
FOR EACH ROW EXECUTE FUNCTION fn_albumtagstats_delete_cleanup();

CREATE OR REPLACE FUNCTION fn_tracktagstats_votes_cleanup()
RETURNS TRIGGER AS $$
DECLARE
    v_stats_id UUID;
BEGIN
    IF TG_OP = 'DELETE' THEN
        v_stats_id := OLD."TrackTagStatsId";
        
        IF NOT EXISTS (SELECT 1 FROM "TrackTagStatsUser" WHERE "TrackTagStatsId" = v_stats_id) THEN
            DELETE FROM "TrackTagStats" WHERE "Id" = v_stats_id;
        END IF;
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tracktagstats_votes_cleanup_trigger ON "TrackTagStatsUser";
CREATE TRIGGER tracktagstats_votes_cleanup_trigger
AFTER DELETE ON "TrackTagStatsUser"
FOR EACH ROW EXECUTE FUNCTION fn_tracktagstats_votes_cleanup();

CREATE OR REPLACE FUNCTION fn_albumtagstats_votes_cleanup()
RETURNS TRIGGER AS $$
DECLARE
    v_stats_id UUID;
BEGIN
    IF TG_OP = 'DELETE' THEN
        v_stats_id := OLD."AlbumTagStatsId";
        
        IF NOT EXISTS (SELECT 1 FROM "AlbumTagStatsUser" WHERE "AlbumTagStatsId" = v_stats_id) THEN
            DELETE FROM "AlbumTagStats" WHERE "Id" = v_stats_id;
        END IF;
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS albumtagstats_votes_cleanup_trigger ON "AlbumTagStatsUser";
CREATE TRIGGER albumtagstats_votes_cleanup_trigger
AFTER DELETE ON "AlbumTagStatsUser"
FOR EACH ROW EXECUTE FUNCTION fn_albumtagstats_votes_cleanup();