using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InterviewScheduling.Api.Services;

public sealed class SchoolSettingsService(AppDbContext db, IOptions<SchedulingOptions> fallbackOptions)
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

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

    public async Task<(string TimeZoneId, DateTimeOffset? UpdatedAt, Guid? UpdatedByDirectorId)> GetSnapshotAsync(
        CancellationToken ct)
    {
        var row = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        var tz = row?.SchoolTimeZoneId;
        if (string.IsNullOrWhiteSpace(tz))
            tz = fallbackOptions.Value.SchoolTimeZoneId;
        return (tz, row?.UpdatedAt, row?.UpdatedByDirectorId);
    }

    public async Task UpdateSchoolTimeZoneAsync(Guid directorId, string timeZoneId, CancellationToken ct)
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

        var row = await db.SchoolSettings.FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row is null)
        {
            db.SchoolSettings.Add(new SchoolSettings
            {
                Id = SingletonId,
                SchoolTimeZoneId = tid,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByDirectorId = directorId
            });
        }
        else
        {
            row.SchoolTimeZoneId = tid;
            row.UpdatedAt = DateTimeOffset.UtcNow;
            row.UpdatedByDirectorId = directorId;
        }

        await db.SaveChangesAsync(ct);
    }
}
