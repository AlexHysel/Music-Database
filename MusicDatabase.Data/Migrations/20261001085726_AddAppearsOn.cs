using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearsOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackArtists_Tracks_TrackId",
                table: "TrackArtists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackArtists",
                table: "TrackArtists");

            migrationBuilder.DropIndex(
                name: "IX_TrackArtists_TrackId",
                table: "TrackArtists");

            migrationBuilder.RenameColumn(
                name: "TrackId",
                table: "TrackArtists",
                newName: "AppearsOnId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackArtists",
                table: "TrackArtists",
                columns: new[] { "AppearsOnId", "OthersId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtists_OthersId",
                table: "TrackArtists",
                column: "OthersId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackArtists_Tracks_AppearsOnId",
                table: "TrackArtists",
                column: "AppearsOnId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackArtists_Tracks_AppearsOnId",
                table: "TrackArtists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackArtists",
                table: "TrackArtists");

            migrationBuilder.DropIndex(
                name: "IX_TrackArtists_OthersId",
                table: "TrackArtists");

            migrationBuilder.RenameColumn(
                name: "AppearsOnId",
                table: "TrackArtists",
                newName: "TrackId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackArtists",
                table: "TrackArtists",
                columns: new[] { "OthersId", "TrackId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtists_TrackId",
                table: "TrackArtists",
                column: "TrackId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackArtists_Tracks_TrackId",
                table: "TrackArtists",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
