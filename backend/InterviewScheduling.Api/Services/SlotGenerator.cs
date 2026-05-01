using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InterviewScheduling.Api.Services;

public sealed class SlotGenerator(AppDbContext db, IOptions<SchedulingOptions> schedulingOptions)
{
    private readonly SchedulingOptions _opt = schedulingOptions.Value;

    public async Task<IReadOnlyList<SlotDto>> GetAvailableSlotsAsync(Guid teacherOfferingId, DateOnly localDate,
        CancellationToken ct)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.SchoolTimeZoneId);
        var offering = await db.TeacherOfferings
            .AsNoTracking()
            .Include(o => o.WeeklyAvailabilities)
            .FirstOrDefaultAsync(o => o.Id == teacherOfferingId, ct);
        if (offering is null)
            throw new KeyNotFoundException("Offering not found.");

        var dow = localDate.DayOfWeek;
        var windows = offering.WeeklyAvailabilities.Where(w => w.DayOfWeek == dow).ToList();
        if (windows.Count == 0)
            return Array.Empty<SlotDto>();

        var dayStartUnspecified = localDate.ToDateTime(TimeOnly.MinValue);
        var dayStartLocal = DateTime.SpecifyKind(dayStartUnspecified, DateTimeKind.Unspecified);
        var dayEndLocal = dayStartLocal.AddDays(1);

        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, tz);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, tz);

        var bookings = await db.Bookings.AsNoTracking()
            .Where(b => b.TeacherOfferingId == teacherOfferingId && b.StartUtc < dayEndUtc && b.EndUtc > dayStartUtc)
            .ToListAsync(ct);

        var slotLength = TimeSpan.FromMinutes(_opt.SlotLengthMinutes);
        var slots = new List<SlotDto>();

        foreach (var w in windows)
        {
            var windowStartLocal = dayStartLocal.Date + w.StartLocal;
            var windowEndLocal = dayStartLocal.Date + w.EndLocal;
            if (windowEndLocal <= windowStartLocal)
                continue;

            for (var t = windowStartLocal; t + slotLength <= windowEndLocal; t += slotLength)
            {
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(t, DateTimeKind.Unspecified), tz);
                var endUtc = startUtc + slotLength;
                if (startUtc < dayStartUtc || endUtc > dayEndUtc)
                    continue;

                var taken = bookings.Any(b => b.StartUtc < endUtc && b.EndUtc > startUtc);
                if (!taken)
                    slots.Add(new SlotDto(startUtc, endUtc));
            }
        }

        slots.Sort((a, b) => a.StartUtc.CompareTo(b.StartUtc));
        return slots;
    }
}

public sealed record SlotDto(DateTime StartUtc, DateTime EndUtc);
