using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddCleanupTriggerSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(File.ReadAllText(Path.Combine("Migrations","SQL","CleanupEmptyAlbumsAndArtists.sql")));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS track_cleanup_trigger ON \"Tracks\";");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_cleanup_empty_album_artist();");
        }
    }
}
