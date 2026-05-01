using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/catalog/teacher-offerings")]
public sealed class SlotsController(SlotGenerator slotGenerator) : ControllerBase
{
    [HttpGet("{offeringId:guid}/slots")]
    [ProducesResponseType(typeof(IReadOnlyList<SlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SlotDto>>> Slots(Guid offeringId, [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        try
        {
            var slots = await slotGenerator.GetAvailableSlotsAsync(offeringId, date, ct);
            return Ok(slots);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
