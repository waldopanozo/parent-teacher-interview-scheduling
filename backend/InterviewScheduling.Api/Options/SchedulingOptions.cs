namespace InterviewScheduling.Api.Options;

public sealed class SchedulingOptions
{
    public const string SectionName = "Scheduling";

    /// <summary>
    /// IANA time zone id used to interpret weekly availability and calendar dates (e.g. America/New_York).
    /// </summary>
    public string SchoolTimeZoneId { get; set; } = "America/New_York";

    /// <summary>Default UI language when no school row exists: <c>en</c> or <c>es</c>.</summary>
    public string UiLanguage { get; set; } = "en";

    public int SlotLengthMinutes { get; set; } = 15;
}
