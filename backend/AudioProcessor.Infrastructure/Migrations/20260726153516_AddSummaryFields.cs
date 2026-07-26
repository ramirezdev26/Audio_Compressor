using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AudioProcessor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSummaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "AudioFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SummaryTimeMs",
                table: "AudioFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "AudioFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "TranscriptionTimeMs",
                table: "AudioFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "SummaryTimeMs",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "Transcript",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "TranscriptionTimeMs",
                table: "AudioFiles");
        }
    }
}
