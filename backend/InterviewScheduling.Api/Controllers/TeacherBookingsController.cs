using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize(Roles = "Teacher")]
[Route("api/v1/teacher/bookings")]
public sealed class TeacherBookingsController(BookingService bookingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> Mine(CancellationToken ct)
    {
        var teacherId = User.GetUserId();
        var rows = await bookingService.ListForTeacherAsync(teacherId, ct);
        return Ok(rows);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> UpdateOutcome(Guid id, [FromBody] TeacherUpdateBookingRequest body,
        CancellationToken ct)
    {
        try
        {
            var dto = await bookingService.UpdateTeacherBookingOutcomeAsync(User.GetUserId(), id, body.AttendanceStatus,
                body.VisitNotes, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
