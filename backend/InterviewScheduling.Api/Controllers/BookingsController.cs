using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize(Roles = "Parent")]
[Route("api/v1/parent/bookings")]
public sealed class BookingsController(BookingService bookingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> Mine(CancellationToken ct)
    {
        var parentId = User.GetUserId();
        var rows = await bookingService.ListForParentAsync(parentId, ct);
        return Ok(rows);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest body, CancellationToken ct)
    {
        var parentId = User.GetUserId();
        try
        {
            var booking = await bookingService.CreateParentBookingAsync(parentId, body.TeacherOfferingId, body.StartUtc,
                ct);
            return CreatedAtAction(nameof(Mine), booking);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
