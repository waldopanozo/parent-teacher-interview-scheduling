using InterviewScheduling.Api.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewScheduling.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> GoogleSignIn([FromBody] GoogleSignInRequest body,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.IdToken))
            return BadRequest("idToken is required.");

        try
        {
            var result = await authService.SignInWithGoogleAsync(body.IdToken, ct);
            return Ok(new AuthResponseDto(result.AccessToken, result.ExpiresAtUtc, result.User));
        }
        catch (AuthException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken ct)
    {
        var dto = await authService.GetUserProfileAsync(User.GetUserId(), ct);
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }
}
