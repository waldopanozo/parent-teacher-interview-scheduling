using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InterviewScheduling.Api.Tests;

public sealed class ParentMeetingProfileApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ParentMeetingProfileApiTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetMeetingProfile_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/v1/parent/meeting-profile");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task PutMeetingProfile_valid_then_get_round_trips()
    {
        var parentId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = parentId,
                GoogleSub = "test-google-sub-" + parentId,
                Email = "parent.integration@test.local",
                DisplayName = "Integration Parent",
                Role = AppRole.Parent,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateTokenForUser(_factory, parentId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var put = await client.PutAsJsonAsync("/api/v1/parent/meeting-profile", new UpsertParentMeetingProfileRequest
        {
            StudentSchoolEmail = "student.child@school.edu",
            InterviewAttendeeName = "Alex Guardian",
            RelationshipToStudent = "Legal guardian"
        });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var getRes = await client.GetAsync("/api/v1/parent/meeting-profile");
        getRes.EnsureSuccessStatusCode();
        using var getDoc = JsonDocument.Parse(await getRes.Content.ReadAsStringAsync());
        Assert.Equal("student.child@school.edu", getDoc.RootElement.GetProperty("studentSchoolEmail").GetString());
        Assert.Equal("Alex Guardian", getDoc.RootElement.GetProperty("interviewAttendeeName").GetString());
        Assert.Equal("Legal guardian", getDoc.RootElement.GetProperty("relationshipToStudent").GetString());

        var meRes = await client.GetAsync("/api/v1/auth/me");
        meRes.EnsureSuccessStatusCode();
        using var meDoc = JsonDocument.Parse(await meRes.Content.ReadAsStringAsync());
        Assert.True(meDoc.RootElement.GetProperty("meetingProfileComplete").GetBoolean());
    }

    [Fact]
    public async Task PutMeetingProfile_invalid_email_returns_400()
    {
        var parentId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = parentId,
                GoogleSub = "test-google-sub-bad-" + parentId,
                Email = "parent.bad@test.local",
                DisplayName = "Bad Parent",
                Role = AppRole.Parent,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateTokenForUser(_factory, parentId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var put = await client.PutAsJsonAsync("/api/v1/parent/meeting-profile", new UpsertParentMeetingProfileRequest
        {
            StudentSchoolEmail = "not-valid",
            InterviewAttendeeName = "Someone",
            RelationshipToStudent = "Father"
        });
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    private static string CreateTokenForUser(WebApplicationFactory<Program> factory, Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.AsNoTracking().First(u => u.Id == userId);
        return jwt.CreateAccessToken(userId, user.Email, AppRole.Parent);
    }
}
