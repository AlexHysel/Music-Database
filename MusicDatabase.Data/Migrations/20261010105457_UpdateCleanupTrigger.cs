using Microsoft.EntityFrameworkCore.Migrations;
using MusicDatabase.Data;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCleanupTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(MigrationHelper.Read("CleanupEmptyAlbumsAndArtists.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trackartists_cleanup_trigger ON "TrackArtists";
                DROP TRIGGER IF EXISTS album_cleanup_trigger ON "Albums";
                DROP TRIGGER IF EXISTS track_cleanup_trigger ON "Tracks";
                DROP FUNCTION IF EXISTS fn_trackartists_cleanup();
                DROP FUNCTION IF EXISTS fn_albums_cleanup();
                DROP FUNCTION IF EXISTS fn_tracks_cleanup();
                DROP FUNCTION IF EXISTS fn_delete_artist_if_orphan(UUID);
                """);
        }
    }
}
