using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewScheduling.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchoolThemeAndLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "SchoolSettings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LogoData",
                table: "SchoolSettings",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemePreset",
                table: "SchoolSettings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "SchoolSettings");

            migrationBuilder.DropColumn(
                name: "LogoData",
                table: "SchoolSettings");

            migrationBuilder.DropColumn(
                name: "ThemePreset",
                table: "SchoolSettings");
        }
    }
}
