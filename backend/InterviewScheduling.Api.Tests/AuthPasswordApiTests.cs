using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InterviewScheduling.Api.Tests;

public sealed class AuthPasswordApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthPasswordApiTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Me_includes_hasPasswordLogin_false_for_google_only_user()
    {
        var userId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = userId,
                GoogleSub = "google-sub-" + userId,
                PasswordHash = null,
                Email = "google.only@test.local",
                DisplayName = "Google Only",
                Role = AppRole.Parent,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateToken(_factory, userId, AppRole.Parent);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var me = await client.GetAsync("/api/v1/auth/me");
        me.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        Assert.False(doc.RootElement.GetProperty("hasPasswordLogin").GetBoolean());
    }

    [Fact]
    public async Task Me_includes_hasPasswordLogin_true_for_local_password_user()
    {
        var userId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = userId,
                GoogleSub = "local:" + Guid.NewGuid().ToString("N"),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Email = "local.user@test.local",
                DisplayName = "Local User",
                Role = AppRole.Parent,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateToken(_factory, userId, AppRole.Parent);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var me = await client.GetAsync("/api/v1/auth/me");
        me.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("hasPasswordLogin").GetBoolean());
    }

    [Fact]
    public async Task ChangePassword_wrong_current_returns_400()
    {
        var userId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = userId,
                GoogleSub = "local:" + Guid.NewGuid().ToString("N"),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Email = "chgpw.bad@test.local",
                DisplayName = "Change PW",
                Role = AppRole.Teacher,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateToken(_factory, userId, AppRole.Teacher);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PutAsJsonAsync("/api/v1/auth/password", new
        {
            currentPassword = "not-the-password",
            newPassword = "newpass12345"
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_success_then_login_with_new_password()
    {
        const string email = "chgpw.ok@test.local";
        var userId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = userId,
                GoogleSub = "local:" + Guid.NewGuid().ToString("N"),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass123"),
                Email = email,
                DisplayName = "OK User",
                Role = AppRole.Parent,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateToken(_factory, userId, AppRole.Parent);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var put = await client.PutAsJsonAsync("/api/v1/auth/password", new
        {
            currentPassword = "oldpass123",
            newPassword = "newpass12345"
        });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var anon = _factory.CreateClient();
        var bad = await anon.PostAsJsonAsync("/api/v1/auth/email-login", new { email, password = "oldpass123" });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);

        var ok = await anon.PostAsJsonAsync("/api/v1/auth/email-login", new { email, password = "newpass12345" });
        ok.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ChangePassword_google_only_returns_400()
    {
        var userId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new AppUser
            {
                Id = userId,
                GoogleSub = "google-only-" + userId,
                PasswordHash = null,
                Email = "no.pw@test.local",
                DisplayName = "No PW",
                Role = AppRole.Parent,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var token = CreateToken(_factory, userId, AppRole.Parent);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PutAsJsonAsync("/api/v1/auth/password", new
        {
            currentPassword = "",
            newPassword = "whatever12345"
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    private static string CreateToken(ApiWebApplicationFactory factory, Guid userId, AppRole role)
    {
        using var scope = factory.Services.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.AsNoTracking().First(u => u.Id == userId);
        return jwt.CreateAccessToken(userId, user.Email, role);
    }
}
