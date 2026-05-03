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

    /// <summary>Default color palette when no school row exists.</summary>
    public string ThemePreset { get; set; } = "default";

    public int SlotLengthMinutes { get; set; } = 15;

    /// <summary>Maximum school logo upload size for <c>POST /director/school-logo</c> (kilobytes). Clamped at runtime (64–8192).</summary>
    public int MaxSchoolLogoKb { get; set; } = 1024;
}
