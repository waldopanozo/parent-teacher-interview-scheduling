using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InterviewScheduling.Api.Services;

public sealed class SchoolSettingsService(AppDbContext db, IOptions<SchedulingOptions> fallbackOptions)
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public const int MaxLogoBytes = 512 * 1024;

    private static readonly HashSet<string> SupportedUiLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "es"
    };

    private static readonly HashSet<string> SupportedLogoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/svg+xml",
        "image/webp"
    };

    public async Task<TimeZoneInfo> GetSchoolTimeZoneAsync(CancellationToken ct)
    {
        var id = await GetSchoolTimeZoneIdAsync(ct);
        return TimeZoneInfo.FindSystemTimeZoneById(id);
    }

    public async Task<string> GetSchoolTimeZoneIdAsync(CancellationToken ct)
    {
        var row = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is not null && !string.IsNullOrWhiteSpace(row.SchoolTimeZoneId))
            return row.SchoolTimeZoneId;
        return fallbackOptions.Value.SchoolTimeZoneId;
    }

    public async Task<string> GetSchoolUiLanguageAsync(CancellationToken ct)
    {
        var row = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is not null && !string.IsNullOrWhiteSpace(row.UiLanguage))
            return NormalizeUiLanguage(row.UiLanguage);
        return NormalizeUiLanguage(fallbackOptions.Value.UiLanguage);
    }

    public async Task<string> GetThemePresetAsync(CancellationToken ct)
    {
        var row = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is not null && !string.IsNullOrWhiteSpace(row.ThemePreset))
            return SchoolThemePreset.Normalize(row.ThemePreset);
        return SchoolThemePreset.Normalize(fallbackOptions.Value.ThemePreset);
    }

    public async Task<(byte[]? Bytes, string? ContentType)> GetLogoAsync(CancellationToken ct)
    {
        var row = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row?.LogoData is not { Length: > 0 })
            return (null, null);
        return (row.LogoData, row.LogoContentType);
    }

    public async Task<(string TimeZoneId, string UiLanguage, string ThemePreset, bool HasCustomLogo, long BrandingVersion,
            DateTimeOffset? UpdatedAt, Guid? UpdatedByDirectorId)>
        GetSnapshotAsync(CancellationToken ct)
    {
        var row = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        var tz = row?.SchoolTimeZoneId;
        if (string.IsNullOrWhiteSpace(tz))
            tz = fallbackOptions.Value.SchoolTimeZoneId;
        var lang = row?.UiLanguage;
        if (string.IsNullOrWhiteSpace(lang))
            lang = fallbackOptions.Value.UiLanguage;
        var theme = row?.ThemePreset;
        if (string.IsNullOrWhiteSpace(theme))
            theme = fallbackOptions.Value.ThemePreset;
        theme = SchoolThemePreset.Normalize(theme);
        var hasLogo = row?.LogoData is { Length: > 0 };
        var brandingVersion = row?.UpdatedAt.ToUnixTimeMilliseconds() ?? 0L;
        return (
            TimeZoneId: tz,
            UiLanguage: NormalizeUiLanguage(lang),
            ThemePreset: theme,
            HasCustomLogo: hasLogo,
            BrandingVersion: brandingVersion,
            UpdatedAt: row?.UpdatedAt,
            UpdatedByDirectorId: row?.UpdatedByDirectorId);
    }

    public async Task UpdateSchoolSettingsAsync(Guid directorId, string timeZoneId, string uiLanguage, string themePreset,
        CancellationToken ct)
    {
        var tid = timeZoneId.Trim();
        if (tid.Length == 0)
            throw new ArgumentException("Time zone is required.");

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(tid);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ArgumentException($"Unknown or invalid IANA time zone id: {tid}", ex);
        }

        var lang = NormalizeUiLanguage(uiLanguage);
        if (!SupportedUiLanguages.Contains(lang))
            throw new ArgumentException("UiLanguage must be 'en' or 'es'.");

        string theme;
        if (string.IsNullOrWhiteSpace(themePreset))
            theme = SchoolThemePreset.Default;
        else if (!SchoolThemePreset.All.Contains(themePreset.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Unknown theme preset: {themePreset}");
        else
            theme = SchoolThemePreset.All.First(a => a.Equals(themePreset.Trim(), StringComparison.OrdinalIgnoreCase));

        var row = await db.SchoolSettings.FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is null)
        {
            db.SchoolSettings.Add(new SchoolSettings
            {
                Id = SingletonId,
                SchoolTimeZoneId = tid,
                UiLanguage = lang,
                ThemePreset = theme,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByDirectorId = directorId
            });
        }
        else
        {
            row.SchoolTimeZoneId = tid;
            row.UiLanguage = lang;
            row.ThemePreset = theme;
            row.UpdatedAt = DateTimeOffset.UtcNow;
            row.UpdatedByDirectorId = directorId;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task SetSchoolLogoAsync(Guid directorId, byte[] imageBytes, string contentType, CancellationToken ct)
    {
        if (imageBytes.Length == 0)
            throw new ArgumentException("Logo file is empty.");
        if (imageBytes.Length > MaxLogoBytes)
            throw new ArgumentException($"Logo must be at most {MaxLogoBytes / 1024} KB.");
        var ctNorm = contentType.Trim().ToLowerInvariant();
        if (!SupportedLogoContentTypes.Contains(ctNorm))
            throw new ArgumentException("Logo must be PNG, JPEG, SVG, or WebP.");

        var row = await db.SchoolSettings.FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is null)
        {
            db.SchoolSettings.Add(new SchoolSettings
            {
                Id = SingletonId,
                SchoolTimeZoneId = fallbackOptions.Value.SchoolTimeZoneId,
                UiLanguage = NormalizeUiLanguage(fallbackOptions.Value.UiLanguage),
                ThemePreset = SchoolThemePreset.Normalize(fallbackOptions.Value.ThemePreset),
                LogoData = imageBytes,
                LogoContentType = ctNorm,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByDirectorId = directorId
            });
        }
        else
        {
            row.LogoData = imageBytes;
            row.LogoContentType = ctNorm;
            row.UpdatedAt = DateTimeOffset.UtcNow;
            row.UpdatedByDirectorId = directorId;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task ClearSchoolLogoAsync(Guid directorId, CancellationToken ct)
    {
        var row = await db.SchoolSettings.FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is null)
            return;
        row.LogoData = null;
        row.LogoContentType = null;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedByDirectorId = directorId;
        await db.SaveChangesAsync(ct);
    }

    private static string NormalizeUiLanguage(string raw)
    {
        var s = raw.Trim().ToLowerInvariant();
        if (s.StartsWith("es", StringComparison.Ordinal))
            return "es";
        return "en";
    }
}
