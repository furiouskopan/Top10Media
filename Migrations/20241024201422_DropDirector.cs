using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Top10MediaApi.Migrations
{
    /// <inheritdoc />
    public partial class DropDirector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Director",
                table: "Movies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Movies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
