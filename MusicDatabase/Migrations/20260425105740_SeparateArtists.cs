using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SeparateArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackArtists_Artists_ArtistsId",
                table: "TrackArtists");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackArtists_Tracks_TracksId",
                table: "TrackArtists");

            migrationBuilder.RenameColumn(
                name: "TracksId",
                table: "TrackArtists",
                newName: "TrackId");

            migrationBuilder.RenameColumn(
                name: "ArtistsId",
                table: "TrackArtists",
                newName: "OthersId");

            migrationBuilder.RenameIndex(
                name: "IX_TrackArtists_TracksId",
                table: "TrackArtists",
                newName: "IX_TrackArtists_TrackId");

            migrationBuilder.AddColumn<Guid>(
                name: "ArtistId",
                table: "Tracks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_ArtistId",
                table: "Tracks",
                column: "ArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackArtists_Artists_OthersId",
                table: "TrackArtists",
                column: "OthersId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackArtists_Tracks_TrackId",
                table: "TrackArtists",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Artists_ArtistId",
                table: "Tracks",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackArtists_Artists_OthersId",
                table: "TrackArtists");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackArtists_Tracks_TrackId",
                table: "TrackArtists");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Artists_ArtistId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_ArtistId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "Tracks");

            migrationBuilder.RenameColumn(
                name: "TrackId",
                table: "TrackArtists",
                newName: "TracksId");

            migrationBuilder.RenameColumn(
                name: "OthersId",
                table: "TrackArtists",
                newName: "ArtistsId");

            migrationBuilder.RenameIndex(
                name: "IX_TrackArtists_TrackId",
                table: "TrackArtists",
                newName: "IX_TrackArtists_TracksId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackArtists_Artists_ArtistsId",
                table: "TrackArtists",
                column: "ArtistsId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackArtists_Tracks_TracksId",
                table: "TrackArtists",
                column: "TracksId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
