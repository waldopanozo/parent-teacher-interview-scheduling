using InterviewScheduling.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InterviewScheduling.Api.Data;

public static class DbSeeder
{
    /// <summary>Demo password for seeded accounts (email/password login).</summary>
    public const string DemoPassword = "password";

    /// <param name="seedDemoPasswordUsers">When true, ensures Parent/Teacher/Director demo logins (never enabled in Production).</param>
    public static async Task SeedAsync(AppDbContext db, bool seedDemoPasswordUsers, ILogger logger)
    {
        var subjectsAdded = await SeedSubjectsIfEmptyAsync(db);
        if (subjectsAdded > 0)
            logger.LogInformation("Seeded {Count} subjects (MATH, SCI, ENG, SOC).", subjectsAdded);

        if (!seedDemoPasswordUsers)
        {
            logger.LogInformation("Skipping demo password users (Seed:DemoUsers=false or Production, or not Development).");
            return;
        }

        var demosAdded = await SeedDemoPasswordUsersAsync(db);
        if (demosAdded > 0)
            logger.LogInformation(
                "Seeded {Count} demo users (demo-parent|teacher|director@example.com). Password is documented in README / .env.example.",
                demosAdded);
        else
            logger.LogInformation("Demo users already present; no new demo accounts inserted.");

        await SeedDemoTeacherOfferingIfNeededAsync(db, logger);
    }

    /// <summary>
    /// Ensures <c>demo-teacher@example.com</c> has one MATH offering with Mon–Fri windows (local school time)
    /// so the parent catalog shows bookable slots without using the Director UI first.
    /// </summary>
    private static async Task SeedDemoTeacherOfferingIfNeededAsync(AppDbContext db, ILogger logger)
    {
        const string teacherEmail = "demo-teacher@example.com";
        var teacher = await db.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == teacherEmail && u.Role == AppRole.Teacher);
        if (teacher is null)
        {
            logger.LogWarning("Demo teacher {Email} not found; skipping demo teacher offering.", teacherEmail);
            return;
        }

        if (await db.TeacherOfferings.AnyAsync(o => o.TeacherUserId == teacher.Id))
        {
            logger.LogInformation("Demo teacher already has at least one offering; skipping demo catalog seed.");
            return;
        }

        var subject = await db.Subjects.FirstOrDefaultAsync(s => s.Code == "MATH");
        if (subject is null)
        {
            logger.LogWarning("Subject MATH missing; cannot seed demo teacher offering.");
            return;
        }

        var offering = new TeacherOffering
        {
            Id = Guid.NewGuid(),
            TeacherUserId = teacher.Id,
            SubjectId = subject.Id,
            CourseTitle = "Algebra I",
            GradeLevel = "9",
            SectionLabel = "A"
        };
        db.TeacherOfferings.Add(offering);
        await db.SaveChangesAsync();

        // Weekday afternoons (America/New_York in SlotGenerator) → slots on any Mon–Fri date the parent picks.
        var workdays = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        };
        foreach (var day in workdays)
        {
            db.WeeklyAvailabilities.Add(new WeeklyAvailability
            {
                Id = Guid.NewGuid(),
                TeacherOfferingId = offering.Id,
                DayOfWeek = day,
                StartLocal = new TimeSpan(14, 0, 0),
                EndLocal = new TimeSpan(17, 0, 0)
            });
        }

        await db.SaveChangesAsync();
        logger.LogInformation(
            "Seeded demo teacher offering {OfferingId} (Algebra I, grade 9, section A) with Mon–Fri 14:00–17:00 availability.",
            offering.Id);
    }

    private static async Task<int> SeedSubjectsIfEmptyAsync(AppDbContext db)
    {
        if (await db.Subjects.AnyAsync())
            return 0;

        db.Subjects.AddRange(
            new Subject { Id = Guid.NewGuid(), Code = "MATH", Name = "Mathematics" },
            new Subject { Id = Guid.NewGuid(), Code = "SCI", Name = "Science" },
            new Subject { Id = Guid.NewGuid(), Code = "ENG", Name = "English Language Arts" },
            new Subject { Id = Guid.NewGuid(), Code = "SOC", Name = "Social Studies" });

        await db.SaveChangesAsync();
        return 4;
    }

    /// <summary>One user per role for local/demo; skipped if the email already exists.</summary>
    private static async Task<int> SeedDemoPasswordUsersAsync(AppDbContext db)
    {
        var demos = new (string Email, string DisplayName, AppRole Role)[]
        {
            ("demo-parent@example.com", "Demo Parent", AppRole.Parent),
            ("demo-teacher@example.com", "Demo Teacher", AppRole.Teacher),
            ("demo-director@example.com", "Demo Director", AppRole.Director)
        };

        var added = 0;
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
            added++;
        }

        if (added > 0)
            await db.SaveChangesAsync();

        return added;
    }
}
