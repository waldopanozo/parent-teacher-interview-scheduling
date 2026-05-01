using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize(Roles = "Parent")]
[Route("api/v1/parent/meeting-profile")]
public sealed class ParentMeetingProfileController(ParentMeetingProfileService profiles) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ParentMeetingProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ParentMeetingProfileDto>> Get(CancellationToken ct)
    {
        var dto = await profiles.GetAsync(User.GetUserId(), ct);
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upsert([FromBody] UpsertParentMeetingProfileRequest body, CancellationToken ct)
    {
        var result = await profiles.UpsertAsync(User.GetUserId(), body, ct);
        if (!result.Ok)
            return BadRequest(result.Error);
        return NoContent();
    }
}
