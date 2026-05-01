using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewScheduling.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DirectorTeacherAccessAndSectionLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionLabel",
                table: "TeacherOfferings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TeacherAccessRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAccessRequests_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherAccessRequests_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAccessRequests_ApplicantUserId",
                table: "TeacherAccessRequests",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAccessRequests_DecidedByUserId",
                table: "TeacherAccessRequests",
                column: "DecidedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherAccessRequests");

            migrationBuilder.DropColumn(
                name: "SectionLabel",
                table: "TeacherOfferings");
        }
    }
}
