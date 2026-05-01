using InterviewScheduling.Api.Auth;
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
}
