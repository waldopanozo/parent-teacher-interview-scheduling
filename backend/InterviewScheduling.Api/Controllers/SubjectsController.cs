using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/subjects")]
public sealed class SubjectsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubjectSummaryDto>>> List(CancellationToken ct)
    {
        var rows = await db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);
        var dto = rows.Select(s => new SubjectSummaryDto(s.Id, s.Code, s.Name)).ToList();
        return Ok(dto);
    }
}
