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
        var s = await schoolSettings.GetSnapshotAsync(ct);
        return Ok(new SchoolPublicConfigDto(s.TimeZoneId, s.UiLanguage, s.ThemePreset, s.HasCustomLogo,
            s.BrandingVersion));
    }

    [HttpGet("school-logo")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchoolLogo(CancellationToken ct)
    {
        var (bytes, contentType) = await schoolSettings.GetLogoAsync(ct);
        if (bytes is null || bytes.Length == 0)
            return NotFound();
        return File(bytes, contentType ?? "application/octet-stream");
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
