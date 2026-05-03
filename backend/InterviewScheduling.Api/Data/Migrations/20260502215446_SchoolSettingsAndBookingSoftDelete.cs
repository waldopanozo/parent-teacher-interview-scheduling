using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewScheduling.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchoolSettingsAndBookingSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TeacherOfferingId_StartUtc",
                table: "Bookings");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchoolSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolTimeZoneId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByDirectorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TeacherOfferingId_StartUtc",
                table: "Bookings",
                columns: new[] { "TeacherOfferingId", "StartUtc" },
                unique: true,
                filter: "\"CancelledAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolSettings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TeacherOfferingId_StartUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TeacherOfferingId_StartUtc",
                table: "Bookings",
                columns: new[] { "TeacherOfferingId", "StartUtc" },
                unique: true);
        }
    }
}
