using Google.Apis.Auth;
using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace InterviewScheduling.Api.Services;

public sealed class AuthService(
    AppDbContext db,
    IConfiguration configuration,
    IOptions<AuthOptions> authOptions,
    IOptions<JwtOptions> jwtOptions,
    JwtTokenService jwtTokenService,
    ILogger<AuthService> logger)
{
    private readonly AuthOptions _auth = authOptions.Value;
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<AuthResult> SignInWithGoogleAsync(string idToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var clientId = configuration["Google:WebClientId"]
                       ?? Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Configure Google:WebClientId or GOOGLE_OAUTH_CLIENT_ID.");

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [clientId]
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Rejected Google id_token");
            throw new AuthException("Invalid Google credential.");
        }

        var email = (payload.Email ?? "").Trim();
        if (email.Length == 0 || payload.EmailVerified != true)
            throw new AuthException("Google account email is missing or not verified.");

        if (!IsAllowedSignInEmail(email, payload.HostedDomain))
            throw new AuthException("Email address is not permitted for this school configuration.");

        var sub = payload.Subject;
        var displayName = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name.Trim();

        var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleSub == sub, ct);
        if (user is null)
        {
            var emailKey = email.Trim().ToLowerInvariant();
            var taken = await db.Users.AsNoTracking().AnyAsync(
                u => u.Email.ToLower() == emailKey && u.GoogleSub != sub,
                ct);
            if (taken)
                throw new AuthException(
                    "This email is already registered with email and password. Sign in that way instead.");

            var role = ResolveBootstrapRole(email);
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                GoogleSub = sub,
                Email = email,
                DisplayName = displayName,
                Role = role,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            user.DisplayName = displayName;
            user.Email = email;
            await db.SaveChangesAsync(ct);
        }

        var token = jwtTokenService.CreateAccessToken(user.Id, user.Email, user.Role);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        return new AuthResult(token, expires, MapUserProfile(user));
    }

    public async Task<AuthResult> RegisterWithPasswordAsync(string email, string password, string displayName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        email = email.Trim();
        displayName = displayName.Trim();
        if (email.Length == 0)
            throw new AuthException("Email is required.");
        if (displayName.Length == 0)
            throw new AuthException("Username is required.");
        if (password.Length < 8)
            throw new AuthException("Password must be at least 8 characters.");

        if (!IsAllowedSignInEmail(email, hostedDomain: null))
            throw new AuthException("Email address is not permitted for this school configuration.");

        var emailKey = email.ToLowerInvariant();
        var exists = await db.Users.AsNoTracking().AnyAsync(u => u.Email.ToLower() == emailKey, ct);
        if (exists)
            throw new AuthException("An account with this email already exists.");

        var role = ResolveBootstrapRole(email);
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            GoogleSub = "local:" + Guid.NewGuid().ToString("N"),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email,
            DisplayName = displayName,
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = jwtTokenService.CreateAccessToken(user.Id, user.Email, user.Role);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        return new AuthResult(token, expires, MapUserProfile(user));
    }

    public async Task<AuthResult> SignInWithPasswordAsync(string email, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        email = email.Trim();
        if (email.Length == 0 || password.Length == 0)
            throw new AuthException("Invalid email or password.");

        var emailKey = email.ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailKey, ct);
        if (user is null || user.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new AuthException("Invalid email or password.");

        var token = jwtTokenService.CreateAccessToken(user.Id, user.Email, user.Role);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        return new AuthResult(token, expires, MapUserProfile(user));
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : MapUserProfile(user);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword,
        CancellationToken ct)
    {
        if (newPassword.Length < 8)
            throw new AuthException("New password must be at least 8 characters.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            throw new AuthException("User not found.");
        if (user.PasswordHash is null)
            throw new AuthException(
                "This account uses Google sign-in only. Password change is not available.");

        if (string.IsNullOrEmpty(currentPassword) ||
            !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new AuthException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await db.SaveChangesAsync(ct);
    }

    private static UserProfileDto MapUserProfile(AppUser user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role, MeetingProfileValidation.IsComplete(user),
            user.PasswordHash is not null);

    private AppRole ResolveBootstrapRole(string email)
    {
        if (ParseEmailSet(_auth.DirectorBootstrapEmails).Contains(email, StringComparer.OrdinalIgnoreCase))
            return AppRole.Director;
        if (ParseEmailSet(_auth.TeacherBootstrapEmails).Contains(email, StringComparer.OrdinalIgnoreCase))
            return AppRole.Teacher;
        return AppRole.Parent;
    }

    private static HashSet<string> ParseEmailSet(string raw)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            result.Add(part);
        return result;
    }

    private bool IsAllowedSignInEmail(string email, string? hostedDomain)
    {
        var at = email.LastIndexOf('@');
        if (at <= 0 || at == email.Length - 1)
            return false;

        var suffix = email[(at + 1)..].ToLowerInvariant();

        var domains = _auth.AllowedEmailDomains
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(d => d.Trim().TrimStart('@').ToLowerInvariant())
            .Where(d => d.Length > 0)
            .ToHashSet();

        if (domains.Count == 0)
            return true;

        if (_auth.AllowPersonalGoogleEmails && IsConsumerGmailDomain(suffix))
            return true;

        if (domains.Contains(suffix))
            return true;

        if (!string.IsNullOrEmpty(hostedDomain) && domains.Contains(hostedDomain.ToLowerInvariant()))
            return true;

        return false;
    }

    private static bool IsConsumerGmailDomain(string emailDomainSuffix) =>
        emailDomainSuffix is "gmail.com" or "googlemail.com";
}

public sealed record AuthResult(string AccessToken, DateTime ExpiresAtUtc, UserProfileDto User);

public sealed class AuthException(string message) : Exception(message);
