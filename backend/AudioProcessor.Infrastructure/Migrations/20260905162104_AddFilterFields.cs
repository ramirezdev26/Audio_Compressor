using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AudioProcessor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FilterTimeMs",
                table: "AudioFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FilteredUrl",
                table: "AudioFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilterTimeMs",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "FilteredUrl",
                table: "AudioFiles");
        }
    }
}
