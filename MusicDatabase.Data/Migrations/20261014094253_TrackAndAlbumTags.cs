using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class TrackAndAlbumTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackTags");

            migrationBuilder.CreateTable(
                name: "AlbumTagVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaggedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTagVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumTagVotes_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTagVotes_Tags_Id",
                        column: x => x.Id,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTagVotes_Users_TaggedById",
                        column: x => x.TaggedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTagVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaggedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTagVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackTagVotes_Tags_Id",
                        column: x => x.Id,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTagVotes_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTagVotes_Users_TaggedById",
                        column: x => x.TaggedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagVotes_AlbumId",
                table: "AlbumTagVotes",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagVotes_TaggedById",
                table: "AlbumTagVotes",
                column: "TaggedById");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagVotes_TaggedById",
                table: "TrackTagVotes",
                column: "TaggedById");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagVotes_TrackId",
                table: "TrackTagVotes",
                column: "TrackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumTagVotes");

            migrationBuilder.DropTable(
                name: "TrackTagVotes");

            migrationBuilder.CreateTable(
                name: "TrackTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaggedById = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackTags_Tags_Id",
                        column: x => x.Id,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTags_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTags_Users_TaggedById",
                        column: x => x.TaggedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackTags_TaggedById",
                table: "TrackTags",
                column: "TaggedById");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTags_TrackId",
                table: "TrackTags",
                column: "TrackId");
        }
    }
}
