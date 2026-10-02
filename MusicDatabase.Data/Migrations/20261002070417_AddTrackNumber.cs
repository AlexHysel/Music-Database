using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberInTheAlbum",
                table: "Tracks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberInTheAlbum",
                table: "Tracks");
        }
    }
}
