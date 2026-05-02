using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize(Roles = "Director")]
[Route("api/v1/director")]
public sealed class DirectorController(
    AppDbContext db,
    WeeklyAvailabilityService weeklyAvailability,
    SchoolSettingsService schoolSettings,
    BookingService bookingService) : ControllerBase
{
    [HttpGet("school-settings")]
    [ProducesResponseType(typeof(SchoolSettingsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SchoolSettingsResponseDto>> GetSchoolSettings(CancellationToken ct)
    {
        var (tz, at, by) = await schoolSettings.GetSnapshotAsync(ct);
        return Ok(new SchoolSettingsResponseDto(tz, at, by));
    }

    [HttpPut("school-settings")]
    [ProducesResponseType(typeof(SchoolSettingsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SchoolSettingsResponseDto>> PutSchoolSettings([FromBody] UpdateSchoolSettingsRequest body,
        CancellationToken ct)
    {
        try
        {
            await schoolSettings.UpdateSchoolTimeZoneAsync(User.GetUserId(), body.SchoolTimeZoneId, ct);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var (tz, at, by) = await schoolSettings.GetSnapshotAsync(ct);
        return Ok(new SchoolSettingsResponseDto(tz, at, by));
    }

    [HttpGet("cancelled-bookings")]
    [ProducesResponseType(typeof(IReadOnlyList<CancelledBookingAuditDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CancelledBookingAuditDto>>> ListCancelledBookings(CancellationToken ct)
    {
        var rows = await bookingService.ListCancelledBookingsAuditAsync(ct);
        return Ok(rows);
    }

    [HttpGet("teachers")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherListItemDto>>> ListTeachers(CancellationToken ct)
    {
        var rows = await db.Users.AsNoTracking()
            .Where(u => u.Role == AppRole.Teacher)
            .OrderBy(u => u.DisplayName)
            .Select(u => new TeacherListItemDto(u.Id, u.Email, u.DisplayName))
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("teacher-offerings")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherOfferingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherOfferingSummaryDto>>> ListOfferings(
        [FromQuery] Guid? teacherUserId, CancellationToken ct)
    {
        var q = db.TeacherOfferings.AsNoTracking()
            .Include(o => o.Teacher)
            .Include(o => o.Subject)
            .AsQueryable();
        if (teacherUserId is { } tid)
            q = q.Where(o => o.TeacherUserId == tid);

        var rows = await q.OrderBy(o => o.Teacher.DisplayName).ThenBy(o => o.Subject.Name).ToListAsync(ct);
        var dto = rows.Select(o => new TeacherOfferingSummaryDto(
            o.Id,
            o.Teacher.DisplayName,
            o.Subject.Name,
            o.Subject.Code,
            o.CourseTitle,
            o.GradeLevel,
            o.SectionLabel)).ToList();
        return Ok(dto);
    }

    [HttpGet("teacher-access-requests")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherAccessRequestListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherAccessRequestListItemDto>>> ListRequests(
        [FromQuery] TeacherAccessRequestStatus? status, CancellationToken ct)
    {
        var q = db.TeacherAccessRequests.AsNoTracking().Include(r => r.Applicant).AsQueryable();
        if (status is { } s)
            q = q.Where(r => r.Status == s);
        var rows = await q.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        var dto = rows.Select(r => new TeacherAccessRequestListItemDto(
            r.Id,
            r.ApplicantUserId,
            r.Applicant.Email,
            r.Applicant.DisplayName,
            r.Message,
            r.Status,
            r.CreatedAt)).ToList();
        return Ok(dto);
    }

    [HttpPost("teacher-access-requests/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApproveRequest(Guid id, CancellationToken ct)
    {
        var directorId = User.GetUserId();
        var req = await db.TeacherAccessRequests.Include(r => r.Applicant)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (req is null)
            return NotFound();
        if (req.Status != TeacherAccessRequestStatus.Pending)
            return BadRequest("Request is not pending.");

        req.Status = TeacherAccessRequestStatus.Approved;
        req.DecidedAt = DateTimeOffset.UtcNow;
        req.DecidedByUserId = directorId;
        req.Applicant.Role = AppRole.Teacher;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("teacher-access-requests/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectRequest(Guid id, CancellationToken ct)
    {
        var directorId = User.GetUserId();
        var req = await db.TeacherAccessRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (req is null)
            return NotFound();
        if (req.Status != TeacherAccessRequestStatus.Pending)
            return BadRequest("Request is not pending.");

        req.Status = TeacherAccessRequestStatus.Rejected;
        req.DecidedAt = DateTimeOffset.UtcNow;
        req.DecidedByUserId = directorId;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("subjects")]
    [ProducesResponseType(typeof(SubjectSummaryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SubjectSummaryDto>> CreateSubject([FromBody] CreateSubjectRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Code) || string.IsNullOrWhiteSpace(body.Name))
            return BadRequest("code and name are required.");

        var code = body.Code.Trim();
        if (await db.Subjects.AnyAsync(s => s.Code == code, ct))
            return Conflict("Subject code already exists.");

        var s = new Subject
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = body.Name.Trim()
        };
        db.Subjects.Add(s);
        await db.SaveChangesAsync(ct);
        return Created($"/api/v1/subjects/{s.Id}", new SubjectSummaryDto(s.Id, s.Code, s.Name));
    }

    [HttpPut("subjects/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Code) || string.IsNullOrWhiteSpace(body.Name))
            return BadRequest("code and name are required.");

        var s = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null)
            return NotFound();

        var code = body.Code.Trim();
        if (await db.Subjects.AnyAsync(x => x.Code == code && x.Id != id, ct))
            return Conflict("Subject code already in use.");

        s.Code = code;
        s.Name = body.Name.Trim();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("subjects/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken ct)
    {
        if (await db.TeacherOfferings.AnyAsync(o => o.SubjectId == id, ct))
            return Conflict("Cannot delete a subject that is still assigned to teacher offerings.");

        var s = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null)
            return NotFound();
        db.Subjects.Remove(s);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("teacher-offerings")]
    [ProducesResponseType(typeof(TeacherOfferingSummaryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TeacherOfferingSummaryDto>> CreateOfferingForTeacher(
        [FromBody] DirectorCreateTeacherOfferingRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.CourseTitle) || string.IsNullOrWhiteSpace(body.GradeLevel))
            return BadRequest("courseTitle and gradeLevel are required.");

        var teacher = await db.Users.FirstOrDefaultAsync(u => u.Id == body.TeacherUserId && u.Role == AppRole.Teacher, ct);
        if (teacher is null)
            return BadRequest("Teacher user not found or is not in the Teacher role.");

        var subject = await db.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == body.SubjectId, ct);
        if (subject is null)
            return BadRequest("Unknown subject.");

        var offering = new TeacherOffering
        {
            Id = Guid.NewGuid(),
            TeacherUserId = body.TeacherUserId,
            SubjectId = body.SubjectId,
            CourseTitle = body.CourseTitle.Trim(),
            GradeLevel = body.GradeLevel.Trim(),
            SectionLabel = (body.SectionLabel ?? "").Trim()
        };
        db.TeacherOfferings.Add(offering);
        await db.SaveChangesAsync(ct);

        var dto = new TeacherOfferingSummaryDto(
            offering.Id,
            teacher.DisplayName,
            subject.Name,
            subject.Code,
            offering.CourseTitle,
            offering.GradeLevel,
            offering.SectionLabel);
        return Created($"/api/v1/director/teacher-offerings/{offering.Id}", dto);
    }

    [HttpPut("teacher-offerings/{offeringId:guid}/weekly-availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReplaceWeeklyAvailabilityForTeacher(Guid offeringId,
        [FromBody] ReplaceWeeklyAvailabilityRequest body, CancellationToken ct)
    {
        var result = await weeklyAvailability.ReplaceAsync(offeringId, null, body, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
                return NotFound();
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
