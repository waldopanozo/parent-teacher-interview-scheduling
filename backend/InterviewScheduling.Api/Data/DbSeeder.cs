using InterviewScheduling.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Data;

public static class DbSeeder
{
    /// <summary>Demo password for seeded accounts (email/password login).</summary>
    public const string DemoPassword = "password";

    /// <param name="seedDemoPasswordUsers">When true, ensures Parent/Teacher/Director demo logins (Development only; never use in production).</param>
    public static async Task SeedAsync(AppDbContext db, bool seedDemoPasswordUsers)
    {
        await SeedSubjectsIfEmptyAsync(db);
        if (seedDemoPasswordUsers)
            await SeedDemoPasswordUsersAsync(db);
    }

    private static async Task SeedSubjectsIfEmptyAsync(AppDbContext db)
    {
        if (await db.Subjects.AnyAsync())
            return;

        db.Subjects.AddRange(
            new Subject { Id = Guid.NewGuid(), Code = "MATH", Name = "Mathematics" },
            new Subject { Id = Guid.NewGuid(), Code = "SCI", Name = "Science" },
            new Subject { Id = Guid.NewGuid(), Code = "ENG", Name = "English Language Arts" },
            new Subject { Id = Guid.NewGuid(), Code = "SOC", Name = "Social Studies" });

        await db.SaveChangesAsync();
    }

    /// <summary>One user per role for local/demo; skipped if the email already exists.</summary>
    private static async Task SeedDemoPasswordUsersAsync(AppDbContext db)
    {
        var demos = new (string Email, string DisplayName, AppRole Role)[]
        {
            ("demo-parent@example.com", "Demo Parent", AppRole.Parent),
            ("demo-teacher@example.com", "Demo Teacher", AppRole.Teacher),
            ("demo-director@example.com", "Demo Director", AppRole.Director)
        };

        foreach (var (email, displayName, role) in demos)
        {
            var key = email.ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email.ToLower() == key);
            if (exists)
                continue;

            db.Users.Add(new AppUser
            {
                Id = Guid.NewGuid(),
                GoogleSub = "local:" + Guid.NewGuid().ToString("N"),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
                Email = email,
                DisplayName = displayName,
                Role = role,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }
}
