using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/catalog")]
public sealed class CatalogController(AppDbContext db) : ControllerBase
{
    [HttpGet("teacher-offerings")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherOfferingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherOfferingSummaryDto>>> ListOfferings(CancellationToken ct)
    {
        var rows = await db.TeacherOfferings.AsNoTracking()
            .Include(o => o.Teacher)
            .Include(o => o.Subject)
            .OrderBy(o => o.Teacher.DisplayName).ThenBy(o => o.Subject.Name)
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
}
