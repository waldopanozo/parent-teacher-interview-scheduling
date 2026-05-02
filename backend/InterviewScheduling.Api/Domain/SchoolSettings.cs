namespace InterviewScheduling.Api.Domain;

/// <summary>Singleton school configuration row (fixed id in <c>SchoolSettingsService</c>).</summary>
public sealed class SchoolSettings
{
    public Guid Id { get; set; }
    public string SchoolTimeZoneId { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByDirectorId { get; set; }
}
