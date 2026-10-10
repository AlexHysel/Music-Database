using Microsoft.EntityFrameworkCore.Migrations;
using MusicDatabase.Data;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AlbumTypeChanging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_CreatorId",
                table: "Playlists");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_CreatorId",
                table: "Playlists",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(MigrationHelper.Read(@"ChangeAlbumType.sql"));
            
            migrationBuilder.Sql(MigrationHelper.Read(@"TrackChangeTrigger.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Users_CreatorId",
                table: "Playlists");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Users_CreatorId",
                table: "Playlists",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS track_change_trigger ON \"Tracks\";");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_update_album_type;");
        }
    }
}
