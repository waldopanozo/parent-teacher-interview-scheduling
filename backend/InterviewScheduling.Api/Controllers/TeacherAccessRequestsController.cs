using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize(Roles = "Parent")]
[Route("api/v1/teacher-access-requests")]
public sealed class TeacherAccessRequestsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit([FromBody] SubmitTeacherAccessRequest body, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        if (user.Role != AppRole.Parent)
            return BadRequest("Only parent accounts may request teacher access.");

        var pending = await db.TeacherAccessRequests.AnyAsync(
            r => r.ApplicantUserId == userId && r.Status == TeacherAccessRequestStatus.Pending, ct);
        if (pending)
            return Conflict("A pending teacher access request already exists.");

        db.TeacherAccessRequests.Add(new TeacherAccessRequest
        {
            Id = Guid.NewGuid(),
            ApplicantUserId = userId,
            Message = string.IsNullOrWhiteSpace(body.Message) ? null : body.Message.Trim(),
            Status = TeacherAccessRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
