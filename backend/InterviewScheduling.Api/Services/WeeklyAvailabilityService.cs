using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Services;

public sealed class WeeklyAvailabilityService(AppDbContext db)
{
    /// <summary>
    /// Replaces all weekly windows for an offering. When <paramref name="teacherUserId"/> is set, the offering must belong to that teacher.
    /// </summary>
    public async Task<(bool Ok, string? Error, int StatusCode)> ReplaceAsync(Guid offeringId, Guid? teacherUserId,
        ReplaceWeeklyAvailabilityRequest body, CancellationToken ct)
    {
        var query = db.TeacherOfferings.Include(o => o.WeeklyAvailabilities).Where(o => o.Id == offeringId);
        if (teacherUserId is { } tid)
            query = query.Where(o => o.TeacherUserId == tid);

        var offering = await query.FirstOrDefaultAsync(ct);
        if (offering is null)
            return (false, null, 404);

        foreach (var w in body.Windows)
        {
            if (string.IsNullOrWhiteSpace(w.StartLocal) || string.IsNullOrWhiteSpace(w.EndLocal))
                return (false, "Each window requires startLocal and endLocal (HH:mm).", 400);

            TimeSpan start;
            TimeSpan end;
            try
            {
                start = TimeOnly.Parse(w.StartLocal).ToTimeSpan();
                end = TimeOnly.Parse(w.EndLocal).ToTimeSpan();
            }
            catch
            {
                return (false, "Invalid time format. Use HH:mm.", 400);
            }

            if (end <= start)
                return (false, "endLocal must be after startLocal.", 400);
        }

        foreach (var w in offering.WeeklyAvailabilities.ToList())
            db.WeeklyAvailabilities.Remove(w);

        foreach (var w in body.Windows)
        {
            var start = TimeOnly.Parse(w.StartLocal).ToTimeSpan();
            var end = TimeOnly.Parse(w.EndLocal).ToTimeSpan();
            db.WeeklyAvailabilities.Add(new WeeklyAvailability
            {
                Id = Guid.NewGuid(),
                TeacherOfferingId = offering.Id,
                DayOfWeek = w.DayOfWeek,
                StartLocal = start,
                EndLocal = end
            });
        }

        await db.SaveChangesAsync(ct);
        return (true, null, 204);
    }
}
