using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AudioProcessor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompressionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompressedUrl",
                table: "AudioFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "CompressionTimeMs",
                table: "AudioFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompressedUrl",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "CompressionTimeMs",
                table: "AudioFiles");
        }
    }
}
