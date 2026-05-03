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

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequest body, CancellationToken ct)
    {
        try
        {
            var result = await authService.RegisterWithPasswordAsync(body.Email, body.Password, body.DisplayName, ct);
            return Ok(new AuthResponseDto(result.AccessToken, result.ExpiresAtUtc, result.User));
        }
        catch (AuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("email-login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> EmailLogin([FromBody] EmailLoginRequest body, CancellationToken ct)
    {
        try
        {
            var result = await authService.SignInWithPasswordAsync(body.Email, body.Password, ct);
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

    /// <summary>Email/password accounts only. Google-only accounts receive 400.</summary>
    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest body, CancellationToken ct)
    {
        try
        {
            await authService.ChangePasswordAsync(User.GetUserId(), body.CurrentPassword, body.NewPassword, ct);
            return NoContent();
        }
        catch (AuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
