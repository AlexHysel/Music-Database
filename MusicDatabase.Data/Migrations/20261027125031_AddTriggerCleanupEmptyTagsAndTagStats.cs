using Microsoft.EntityFrameworkCore.Migrations;
using MusicDatabase.Data;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggerCleanupEmptyTagsAndTagStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(MigrationHelper.Read("CleanupEmptyTagsAndTagStats.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tracktagstats_delete_cleanup_trigger ON "TrackTagStats";
                DROP TRIGGER IF EXISTS albumtagstats_delete_cleanup_trigger ON "AlbumTagStats";
                DROP TRIGGER IF EXISTS tracktagstats_votes_cleanup_trigger ON "TrackTagStatsUser";
                DROP TRIGGER IF EXISTS albumtagstats_votes_cleanup_trigger ON "AlbumTagStatsUser";

                DROP FUNCTION IF EXISTS fn_delete_tag_if_orphan(UUID);
                DROP FUNCTION IF EXISTS fn_tracktagstats_delete_cleanup();
                DROP FUNCTION IF EXISTS fn_albumtagstats_delete_cleanup();
                DROP FUNCTION IF EXISTS fn_tracktagstats_votes_cleanup();
                DROP FUNCTION IF EXISTS fn_albumtagstats_votes_cleanup();
                """);
        }
    }
}
