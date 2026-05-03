namespace InterviewScheduling.Api.Domain;

/// <summary>Singleton school configuration row (fixed id in <c>SchoolSettingsService</c>).</summary>
public sealed class SchoolSettings
{
    public Guid Id { get; set; }
    public string SchoolTimeZoneId { get; set; } = "";

    /// <summary>UI language for the whole school: <c>en</c> or <c>es</c>.</summary>
    public string UiLanguage { get; set; } = "en";

    /// <summary>Built-in palette id (e.g. default, ocean, forest).</summary>
    public string ThemePreset { get; set; } = "default";

    public byte[]? LogoData { get; set; }

    /// <summary>MIME type for <see cref="LogoData"/> (e.g. image/png, image/svg+xml).</summary>
    public string? LogoContentType { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByDirectorId { get; set; }
}
