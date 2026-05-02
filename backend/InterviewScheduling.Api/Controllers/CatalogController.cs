using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/catalog")]
public sealed class CatalogController(AppDbContext db, SchoolSettingsService schoolSettings) : ControllerBase
{
    [HttpGet("school-config")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SchoolPublicConfigDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SchoolPublicConfigDto>> GetSchoolConfig(CancellationToken ct)
    {
        var tz = await schoolSettings.GetSchoolTimeZoneIdAsync(ct);
        var lang = await schoolSettings.GetSchoolUiLanguageAsync(ct);
        return Ok(new SchoolPublicConfigDto(tz, lang));
    }

    [HttpGet("school-time-zone")]
    [ProducesResponseType(typeof(SchoolTimeZonePublicDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SchoolTimeZonePublicDto>> GetSchoolTimeZone(CancellationToken ct)
    {
        var id = await schoolSettings.GetSchoolTimeZoneIdAsync(ct);
        return Ok(new SchoolTimeZonePublicDto(id));
    }

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
            o.GradeLevel,
            o.SectionLabel)).ToList();

        return Ok(dto);
    }
}
