using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InterviewScheduling.Api.Services;

public sealed class BookingService(
    AppDbContext db,
    IOptions<SchedulingOptions> schedulingOptions,
    SchoolSettingsService schoolTime)
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
            var parent = await db.Users.FirstOrDefaultAsync(u => u.Id == parentUserId, ct);
            if (parent is null)
                throw new KeyNotFoundException("Parent not found.");
            if (!MeetingProfileValidation.IsComplete(parent))
                throw new InvalidOperationException(
                    "Complete your meeting profile (student school email, attendee name, relationship to student) before booking.");

            var offering = await db.TeacherOfferings
                .Include(o => o.WeeklyAvailabilities)
                .FirstOrDefaultAsync(o => o.Id == teacherOfferingId, ct);
            if (offering is null)
                throw new KeyNotFoundException("Offering not found.");

            if (offering.TeacherUserId == parentUserId)
                throw new InvalidOperationException("Teachers cannot book their own offering.");

            var exists = await db.Bookings.AnyAsync(
                b => b.TeacherOfferingId == teacherOfferingId && b.StartUtc == startUtc && b.CancelledAt == null, ct);
            if (exists)
                throw new InvalidOperationException("That time slot is no longer available.");

            if (!await SlotIsWithinPublishedAvailabilityAsync(offering, startUtc, endUtc, ct))
                throw new InvalidOperationException("Slot is outside the teacher's published weekly availability.");

            await EnsureParentHasAtMostOneBookingPerSchoolDayAsync(parentUserId, startUtc, ct);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TeacherOfferingId = teacherOfferingId,
                ParentUserId = parentUserId,
                StartUtc = startUtc,
                EndUtc = endUtc,
                CreatedAt = DateTimeOffset.UtcNow,
                StudentSchoolEmail = parent.StudentSchoolEmail!,
                InterviewAttendeeName = parent.InterviewAttendeeName!,
                RelationshipToStudent = parent.RelationshipToStudent!
            };
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            await db.Entry(booking).Reference(b => b.Parent).LoadAsync(ct);
            await db.Entry(booking).Reference(b => b.TeacherOffering).Query().Include(o => o.Subject).Include(o => o.Teacher)
                .LoadAsync(ct);

            return ToDto(booking, await ParentMayCancelBookingAsync(booking.StartUtc, ct));
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException("That time slot is no longer available.");
        }
    }

    /// <summary>
    /// Parent may cancel until (but not including) the interview's calendar day in the school time zone.
    /// Soft delete for audit.
    /// </summary>
    public async Task CancelParentBookingAsync(Guid parentUserId, Guid bookingId, CancellationToken ct)
    {
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null || booking.ParentUserId != parentUserId)
            throw new KeyNotFoundException();

        if (booking.CancelledAt is not null)
            throw new KeyNotFoundException();

        if (!await ParentMayCancelBookingAsync(booking.StartUtc, ct))
            throw new InvalidOperationException(
                "Cancellations are only allowed before the day of the interview (school calendar). On the interview day it is too late to cancel online.");

        booking.CancelledAt = DateTimeOffset.UtcNow;
        booking.CancelledByUserId = parentUserId;
        await db.SaveChangesAsync(ct);
    }

    private async Task<bool> ParentMayCancelBookingAsync(DateTime startUtc, CancellationToken ct)
    {
        var tz = await schoolTime.GetSchoolTimeZoneAsync(ct);
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var interviewLocal = TimeZoneInfo.ConvertTimeFromUtc(NormalizeBookingStartUtc(startUtc), tz);
        return nowLocal.Date < interviewLocal.Date;
    }

    private async Task<bool> SlotIsWithinPublishedAvailabilityAsync(TeacherOffering offering, DateTime startUtc,
        DateTime endUtc, CancellationToken ct)
    {
        var tz = await schoolTime.GetSchoolTimeZoneAsync(ct);
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

    private async Task EnsureParentHasAtMostOneBookingPerSchoolDayAsync(Guid parentUserId, DateTime startUtc,
        CancellationToken ct)
    {
        var tz = await schoolTime.GetSchoolTimeZoneAsync(ct);
        var requestedUtc = NormalizeBookingStartUtc(startUtc);
        var requestedLocalDate = TimeZoneInfo.ConvertTimeFromUtc(requestedUtc, tz).Date;

        var existingStarts = await db.Bookings.AsNoTracking()
            .Where(b => b.ParentUserId == parentUserId && b.CancelledAt == null)
            .Select(b => b.StartUtc)
            .ToListAsync(ct);

        foreach (var existing in existingStarts)
        {
            var existingUtc = NormalizeBookingStartUtc(existing);
            var existingLocalDate = TimeZoneInfo.ConvertTimeFromUtc(existingUtc, tz).Date;
            if (existingLocalDate == requestedLocalDate)
                throw new InvalidOperationException(
                    "Only one interview per school day is allowed. You already have a booking on that date.");
        }
    }

    private static DateTime NormalizeBookingStartUtc(DateTime startUtc) =>
        startUtc.Kind switch
        {
            DateTimeKind.Utc => startUtc,
            DateTimeKind.Local => startUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(startUtc, DateTimeKind.Utc)
        };

    public async Task<IReadOnlyList<BookingDto>> ListForParentAsync(Guid parentUserId, CancellationToken ct)
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Parent)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Subject)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Teacher)
            .Where(b => b.ParentUserId == parentUserId && b.CancelledAt == null)
            .OrderBy(b => b.StartUtc)
            .ToListAsync(ct);

        var list = new List<BookingDto>(rows.Count);
        foreach (var b in rows)
            list.Add(ToDto(b, await ParentMayCancelBookingAsync(b.StartUtc, ct)));
        return list;
    }

    public async Task<IReadOnlyList<BookingDto>> ListForTeacherAsync(Guid teacherUserId, CancellationToken ct)
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Parent)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Subject)
            .Where(b => b.TeacherOffering.TeacherUserId == teacherUserId && b.CancelledAt == null)
            .OrderBy(b => b.StartUtc)
            .ToListAsync(ct);
        return rows.Select(b => ToDto(b, false)).ToList();
    }

    public async Task<IReadOnlyList<CancelledBookingAuditDto>> ListCancelledBookingsAuditAsync(CancellationToken ct)
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Parent)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Subject)
            .Include(b => b.TeacherOffering).ThenInclude(o => o.Teacher)
            .Where(b => b.CancelledAt != null)
            .OrderByDescending(b => b.CancelledAt)
            .Take(250)
            .ToListAsync(ct);

        return rows.Select(b =>
        {
            var o = b.TeacherOffering;
            return new CancelledBookingAuditDto(
                b.Id,
                b.CancelledAt,
                b.CancelledByUserId,
                b.StartUtc,
                b.EndUtc,
                b.Parent.Email,
                b.Parent.DisplayName,
                o.Teacher.DisplayName,
                o.Subject.Name,
                o.CourseTitle,
                o.GradeLevel,
                o.SectionLabel);
        }).ToList();
    }

    private BookingDto ToDto(Booking b, bool canCancel)
    {
        var o = b.TeacherOffering;
        return new BookingDto(
            b.Id,
            b.StartUtc,
            b.EndUtc,
            o.Subject.Name,
            o.CourseTitle,
            o.GradeLevel,
            o.SectionLabel,
            b.StudentSchoolEmail,
            b.InterviewAttendeeName,
            b.RelationshipToStudent,
            o.Teacher.DisplayName,
            b.Parent.DisplayName,
            b.Parent.Email,
            canCancel);
    }
}

public sealed record BookingDto(
    Guid Id,
    DateTime StartUtc,
    DateTime EndUtc,
    string SubjectName,
    string CourseTitle,
    string GradeLevel,
    string SectionLabel,
    string StudentSchoolEmail,
    string InterviewAttendeeName,
    string RelationshipToStudent,
    string TeacherDisplayName,
    string ParentDisplayName,
    string ParentEmail,
    bool CanCancel);
