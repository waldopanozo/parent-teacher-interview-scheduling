using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InterviewScheduling.Api.Services;

public sealed class BookingService(AppDbContext db, IOptions<SchedulingOptions> schedulingOptions)
{
    private readonly SchedulingOptions _opt = schedulingOptions.Value;

    public async Task<BookingDto> CreateParentBookingAsync(Guid parentUserId, Guid teacherOfferingId, DateTime startUtc,
        CancellationToken ct)
    {
        var slotLength = TimeSpan.FromMinutes(_opt.SlotLengthMinutes);
        var endUtc = startUtc + slotLength;

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var offering = await db.TeacherOfferings
                .Include(o => o.WeeklyAvailabilities)
                .FirstOrDefaultAsync(o => o.Id == teacherOfferingId, ct);
            if (offering is null)
                throw new KeyNotFoundException("Offering not found.");

            if (offering.TeacherUserId == parentUserId)
                throw new InvalidOperationException("Teachers cannot book their own offering.");

            var exists = await db.Bookings.AnyAsync(
                b => b.TeacherOfferingId == teacherOfferingId && b.StartUtc == startUtc, ct);
            if (exists)
                throw new InvalidOperationException("That time slot is no longer available.");

            if (!await SlotIsWithinPublishedAvailabilityAsync(offering, startUtc, endUtc, ct))
                throw new InvalidOperationException("Slot is outside the teacher's published weekly availability.");

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TeacherOfferingId = teacherOfferingId,
                ParentUserId = parentUserId,
                StartUtc = startUtc,
                EndUtc = endUtc,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            await db.Entry(booking).Reference(b => b.Parent).LoadAsync(ct);
            await db.Entry(booking).Reference(b => b.TeacherOffering).Query().Include(o => o.Subject).Include(o => o.Teacher)
                .LoadAsync(ct);

            return ToDto(booking);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException("That time slot is no longer available.");
        }
    }

    private async Task<bool> SlotIsWithinPublishedAvailabilityAsync(TeacherOffering offering, DateTime startUtc,
        DateTime endUtc, CancellationToken ct)
    {
        _ = ct;
        var tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.SchoolTimeZoneId);
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, tz);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endUtc, tz);
        if (localEnd - localStart != TimeSpan.FromMinutes(_opt.SlotLengthMinutes))
            return false;

        var dow = localStart.DayOfWeek;
        var windows = offering.WeeklyAvailabilities.Where(w => w.DayOfWeek == dow).ToList();
        if (windows.Count == 0)
            return false;

        var startOfDay = localStart.Date;
        foreach (var w in windows)
        {
            var ws = startOfDay + w.StartLocal;
            var we = startOfDay + w.EndLocal;
            if (localStart >= ws && localEnd <= we)
                return true;
        }

        return false;
    }

    public async Task<IReadOnlyList<BookingDto>> ListForParentAsync(Guid parentUserId, CancellationToken ct)
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Subject)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Teacher)
            .Where(b => b.ParentUserId == parentUserId)
            .OrderBy(b => b.StartUtc)
            .ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<BookingDto>> ListForTeacherAsync(Guid teacherUserId, CancellationToken ct)
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Parent)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Subject)
            .Where(b => b.TeacherOffering.TeacherUserId == teacherUserId)
            .OrderBy(b => b.StartUtc)
            .ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    private static BookingDto ToDto(Booking b)
    {
        var o = b.TeacherOffering;
        return new BookingDto(
            b.Id,
            b.StartUtc,
            b.EndUtc,
            o.Subject.Name,
            o.CourseTitle,
            o.GradeLevel,
            o.Teacher.DisplayName,
            b.Parent.DisplayName,
            b.Parent.Email);
    }
}

public sealed record BookingDto(
    Guid Id,
    DateTime StartUtc,
    DateTime EndUtc,
    string SubjectName,
    string CourseTitle,
    string GradeLevel,
    string TeacherDisplayName,
    string ParentDisplayName,
    string ParentEmail);
