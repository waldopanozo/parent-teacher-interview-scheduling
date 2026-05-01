using InterviewScheduling.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
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
}
