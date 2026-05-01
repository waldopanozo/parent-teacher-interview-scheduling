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
[Authorize(Roles = "Teacher")]
[Route("api/v1/teacher/offerings")]
public sealed class TeacherOfferingsController(AppDbContext db, WeeklyAvailabilityService weeklyAvailability)
    : ControllerBase
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
            o.GradeLevel,
            o.SectionLabel)).ToList();

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

        return Created($"/api/v1/teacher/offerings/{offering.Id}", dto);
    }

    [HttpPut("{offeringId:guid}/weekly-availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReplaceWeeklyAvailability(Guid offeringId,
        [FromBody] ReplaceWeeklyAvailabilityRequest body, CancellationToken ct)
    {
        var teacherId = User.GetUserId();
        var result = await weeklyAvailability.ReplaceAsync(offeringId, teacherId, body, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
                return NotFound();
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
