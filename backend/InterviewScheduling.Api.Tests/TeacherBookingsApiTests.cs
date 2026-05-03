using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InterviewScheduling.Api.Tests;

/// <summary>
/// Regression: <c>ListForTeacherAsync</c> must eager-load <c>TeacherOffering.Teacher</c> so <c>ToDto</c> does not
/// null-reference when serializing teacher display names for the dashboard.
/// </summary>
public sealed class TeacherBookingsApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public TeacherBookingsApiTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetTeacherBookings_returns_parent_and_teacher_display_names()
    {
        var teacherId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var offeringId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.AddRange(
                new AppUser
                {
                    Id = teacherId,
                    GoogleSub = "t-sub-" + teacherId,
                    Email = "teacher.bookings@test.local",
                    DisplayName = "Integration Teacher",
                    Role = AppRole.Teacher,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new AppUser
                {
                    Id = parentId,
                    GoogleSub = "p-sub-" + parentId,
                    Email = "parent.bookings@test.local",
                    DisplayName = "Integration Parent",
                    Role = AppRole.Parent,
                    StudentSchoolEmail = "student@school.edu",
                    InterviewAttendeeName = "Guardian",
                    RelationshipToStudent = "Mother",
                    CreatedAt = DateTimeOffset.UtcNow
                });
            db.Subjects.Add(new Subject
            {
                Id = subjectId,
                Code = "INT",
                Name = "Integration Subject"
            });
            var offering = new TeacherOffering
            {
                Id = offeringId,
                TeacherUserId = teacherId,
                SubjectId = subjectId,
                CourseTitle = "Intro",
                GradeLevel = "10",
                SectionLabel = "A"
            };
            db.TeacherOfferings.Add(offering);
            db.WeeklyAvailabilities.Add(new WeeklyAvailability
            {
                Id = Guid.NewGuid(),
                TeacherOfferingId = offeringId,
                DayOfWeek = DayOfWeek.Monday,
                StartLocal = TimeSpan.FromHours(14),
                EndLocal = TimeSpan.FromHours(15)
            });
            db.Bookings.Add(new Booking
            {
                Id = bookingId,
                TeacherOfferingId = offeringId,
                ParentUserId = parentId,
                StartUtc = DateTime.UtcNow.AddDays(7),
                EndUtc = DateTime.UtcNow.AddDays(7).AddMinutes(15),
                CreatedAt = DateTimeOffset.UtcNow,
                StudentSchoolEmail = "student@school.edu",
                InterviewAttendeeName = "Guardian",
                RelationshipToStudent = "Mother",
                AttendanceStatus = AttendanceStatus.Unspecified
            });
            await db.SaveChangesAsync();
        }

        var token = CreateTeacherToken(_factory, teacherId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync("/api/v1/teacher/bookings");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.GetArrayLength() >= 1);
        var row = doc.RootElement.EnumerateArray().First(b => b.GetProperty("id").GetGuid() == bookingId);
        Assert.Equal("Integration Teacher", row.GetProperty("teacherDisplayName").GetString());
        Assert.Equal("parent.bookings@test.local", row.GetProperty("parentEmail").GetString());
        Assert.Equal("Integration Parent", row.GetProperty("parentDisplayName").GetString());
    }

    private static string CreateTeacherToken(ApiWebApplicationFactory factory, Guid teacherId)
    {
        using var scope = factory.Services.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.AsNoTracking().First(u => u.Id == teacherId);
        return jwt.CreateAccessToken(user.Id, user.Email, AppRole.Teacher);
    }
}
