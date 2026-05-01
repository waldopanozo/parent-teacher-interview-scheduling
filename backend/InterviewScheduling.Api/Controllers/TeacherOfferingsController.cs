using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize(Roles = "Teacher")]
[Route("api/v1/teacher/offerings")]
public sealed class TeacherOfferingsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherOfferingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherOfferingSummaryDto>>> Mine(CancellationToken ct)
    {
        var teacherId = User.GetUserId();
        var rows = await db.TeacherOfferings.AsNoTracking()
            .Include(o => o.Teacher)
            .Include(o => o.Subject)
            .Where(o => o.TeacherUserId == teacherId)
            .OrderBy(o => o.Subject.Name)
            .ToListAsync(ct);

        var dto = rows.Select(o => new TeacherOfferingSummaryDto(
            o.Id,
            o.Teacher.DisplayName,
            o.Subject.Name,
            o.Subject.Code,
            o.CourseTitle,
            o.GradeLevel)).ToList();

        return Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeacherOfferingSummaryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TeacherOfferingSummaryDto>> Create([FromBody] CreateTeacherOfferingRequest body,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.CourseTitle) || string.IsNullOrWhiteSpace(body.GradeLevel))
            return BadRequest("courseTitle and gradeLevel are required.");

        var subject = await db.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == body.SubjectId, ct);
        if (subject is null)
            return BadRequest("Unknown subject.");

        var teacherId = User.GetUserId();
        var teacher = await db.Users.FirstAsync(u => u.Id == teacherId, ct);

        var offering = new TeacherOffering
        {
            Id = Guid.NewGuid(),
            TeacherUserId = teacherId,
            SubjectId = body.SubjectId,
            CourseTitle = body.CourseTitle.Trim(),
            GradeLevel = body.GradeLevel.Trim()
        };
        db.TeacherOfferings.Add(offering);
        await db.SaveChangesAsync(ct);

        var dto = new TeacherOfferingSummaryDto(
            offering.Id,
            teacher.DisplayName,
            subject.Name,
            subject.Code,
            offering.CourseTitle,
            offering.GradeLevel);

        return Created($"/api/v1/teacher/offerings/{offering.Id}", dto);
    }

    [HttpPut("{offeringId:guid}/weekly-availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReplaceWeeklyAvailability(Guid offeringId,
        [FromBody] ReplaceWeeklyAvailabilityRequest body, CancellationToken ct)
    {
        var teacherId = User.GetUserId();
        var offering = await db.TeacherOfferings
            .Include(o => o.WeeklyAvailabilities)
            .FirstOrDefaultAsync(o => o.Id == offeringId && o.TeacherUserId == teacherId, ct);
        if (offering is null)
            return NotFound();

        foreach (var w in offering.WeeklyAvailabilities.ToList())
            db.WeeklyAvailabilities.Remove(w);

        foreach (var w in body.Windows)
        {
            if (string.IsNullOrWhiteSpace(w.StartLocal) || string.IsNullOrWhiteSpace(w.EndLocal))
                return BadRequest("Each window requires startLocal and endLocal (HH:mm).");

            TimeSpan start;
            TimeSpan end;
            try
            {
                start = TimeOnly.Parse(w.StartLocal).ToTimeSpan();
                end = TimeOnly.Parse(w.EndLocal).ToTimeSpan();
            }
            catch
            {
                return BadRequest("Invalid time format. Use HH:mm.");
            }

            if (end <= start)
                return BadRequest("endLocal must be after startLocal.");

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
        return NoContent();
    }
}
