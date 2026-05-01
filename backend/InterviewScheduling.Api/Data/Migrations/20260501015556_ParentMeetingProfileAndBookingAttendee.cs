using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewScheduling.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParentMeetingProfileAndBookingAttendee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InterviewAttendeeName",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipToStudent",
                table: "Users",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentSchoolEmail",
                table: "Users",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewAttendeeName",
                table: "Bookings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelationshipToStudent",
                table: "Bookings",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentSchoolEmail",
                table: "Bookings",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterviewAttendeeName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RelationshipToStudent",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentSchoolEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InterviewAttendeeName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RelationshipToStudent",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StudentSchoolEmail",
                table: "Bookings");
        }
    }
}
