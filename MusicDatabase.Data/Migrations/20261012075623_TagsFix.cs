using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class TagsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackTags_Tracks_TrackId1",
                table: "TrackTags");

            migrationBuilder.DropIndex(
                name: "IX_TrackTags_TrackId1",
                table: "TrackTags");

            migrationBuilder.DropColumn(
                name: "TrackId1",
                table: "TrackTags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrackId1",
                table: "TrackTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackTags_TrackId1",
                table: "TrackTags",
                column: "TrackId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackTags_Tracks_TrackId1",
                table: "TrackTags",
                column: "TrackId1",
                principalTable: "Tracks",
                principalColumn: "Id");
        }
    }
}
