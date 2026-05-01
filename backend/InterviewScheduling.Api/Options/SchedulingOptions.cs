namespace InterviewScheduling.Api.Options;

public sealed class SchedulingOptions
{
    public const string SectionName = "Scheduling";

    /// <summary>
    /// IANA time zone id used to interpret weekly availability and calendar dates (e.g. America/New_York).
    /// </summary>
    public string SchoolTimeZoneId { get; set; } = "America/New_York";

    public int SlotLengthMinutes { get; set; } = 15;
}
