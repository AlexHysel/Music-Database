using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFavoritesAddTagStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumTagVotes");

            migrationBuilder.DropTable(
                name: "TrackTagVotes");

            migrationBuilder.DropTable(
                name: "UserAlbums");

            migrationBuilder.DropTable(
                name: "UserArtists");

            migrationBuilder.DropTable(
                name: "UserTracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_AlbumId",
                table: "Tracks");

            migrationBuilder.CreateTable(
                name: "AlbumTagStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTagStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumTagStats_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTagStats_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTagStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTagStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackTagStats_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTagStats_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumTagStatsUser",
                columns: table => new
                {
                    AlbumTagStatsId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTagStatsUser", x => new { x.AlbumTagStatsId, x.VotedById });
                    table.ForeignKey(
                        name: "FK_AlbumTagStatsUser_AlbumTagStats_AlbumTagStatsId",
                        column: x => x.AlbumTagStatsId,
                        principalTable: "AlbumTagStats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTagStatsUser_Users_VotedById",
                        column: x => x.VotedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTagStatsUser",
                columns: table => new
                {
                    TrackTagStatsId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTagStatsUser", x => new { x.TrackTagStatsId, x.VotedById });
                    table.ForeignKey(
                        name: "FK_TrackTagStatsUser_TrackTagStats_TrackTagStatsId",
                        column: x => x.TrackTagStatsId,
                        principalTable: "TrackTagStats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTagStatsUser_Users_VotedById",
                        column: x => x.VotedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumId_NumberInTheAlbum",
                table: "Tracks",
                columns: new[] { "AlbumId", "NumberInTheAlbum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagStats_AlbumId",
                table: "AlbumTagStats",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagStats_TagId",
                table: "AlbumTagStats",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagStatsUser_VotedById",
                table: "AlbumTagStatsUser",
                column: "VotedById");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagStats_TagId",
                table: "TrackTagStats",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagStats_TrackId",
                table: "TrackTagStats",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagStatsUser_VotedById",
                table: "TrackTagStatsUser",
                column: "VotedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumTagStatsUser");

            migrationBuilder.DropTable(
                name: "TrackTagStatsUser");

            migrationBuilder.DropTable(
                name: "AlbumTagStats");

            migrationBuilder.DropTable(
                name: "TrackTagStats");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_AlbumId_NumberInTheAlbum",
                table: "Tracks");

            migrationBuilder.CreateTable(
                name: "AlbumTagVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaggedById = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
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
                        name: "FK_AlbumTagVotes_Tags_TagId",
                        column: x => x.TagId,
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
                    TaggedById = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTagVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackTagVotes_Tags_TagId",
                        column: x => x.TagId,
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

            migrationBuilder.CreateTable(
                name: "UserAlbums",
                columns: table => new
                {
                    FavoriteAlbumsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAlbums", x => new { x.FavoriteAlbumsId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserAlbums_Albums_FavoriteAlbumsId",
                        column: x => x.FavoriteAlbumsId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAlbums_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserArtists",
                columns: table => new
                {
                    FavoriteArtistsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserArtists", x => new { x.FavoriteArtistsId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserArtists_Artists_FavoriteArtistsId",
                        column: x => x.FavoriteArtistsId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserArtists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTracks",
                columns: table => new
                {
                    FavoriteTracksId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTracks", x => new { x.FavoriteTracksId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserTracks_Tracks_FavoriteTracksId",
                        column: x => x.FavoriteTracksId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTracks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumId",
                table: "Tracks",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagVotes_AlbumId",
                table: "AlbumTagVotes",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagVotes_TaggedById",
                table: "AlbumTagVotes",
                column: "TaggedById");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTagVotes_TagId",
                table: "AlbumTagVotes",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagVotes_TaggedById",
                table: "TrackTagVotes",
                column: "TaggedById");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagVotes_TagId",
                table: "TrackTagVotes",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTagVotes_TrackId",
                table: "TrackTagVotes",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAlbums_UserId",
                table: "UserAlbums",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserArtists_UserId",
                table: "UserArtists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTracks_UserId",
                table: "UserTracks",
                column: "UserId");
        }
    }
}
