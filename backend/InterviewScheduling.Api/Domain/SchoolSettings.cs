namespace InterviewScheduling.Api.Domain;

/// <summary>Singleton school configuration row (fixed id in <c>SchoolSettingsService</c>).</summary>
public sealed class SchoolSettings
{
    public Guid Id { get; set; }
    public string SchoolTimeZoneId { get; set; } = "";

    /// <summary>UI language for the whole school: <c>en</c> or <c>es</c>.</summary>
    public string UiLanguage { get; set; } = "en";

    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByDirectorId { get; set; }
}
