using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class TrackAndAlbumTagsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumTagVotes_Tags_Id",
                table: "AlbumTagVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackTagVotes_Tags_Id",
                table: "TrackTagVotes");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagVotes_TagId",
                table: "TrackTagVotes",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagVotes_TagId",
                table: "AlbumTagVotes",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumTagVotes_Tags_TagId",
                table: "AlbumTagVotes",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackTagVotes_Tags_TagId",
                table: "TrackTagVotes",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumTagVotes_Tags_TagId",
                table: "AlbumTagVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackTagVotes_Tags_TagId",
                table: "TrackTagVotes");

            migrationBuilder.DropIndex(
                name: "IX_TrackTagVotes_TagId",
                table: "TrackTagVotes");

            migrationBuilder.DropIndex(
                name: "IX_AlbumTagVotes_TagId",
                table: "AlbumTagVotes");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumTagVotes_Tags_Id",
                table: "AlbumTagVotes",
                column: "Id",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackTagVotes_Tags_Id",
                table: "TrackTagVotes",
                column: "Id",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
