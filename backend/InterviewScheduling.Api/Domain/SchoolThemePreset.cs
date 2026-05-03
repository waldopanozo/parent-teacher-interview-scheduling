namespace InterviewScheduling.Api.Domain;

/// <summary>Allowed values for <see cref="SchoolSettings.ThemePreset"/>.</summary>
public static class SchoolThemePreset
{
    public const string Default = "default";
    public const string Ocean = "ocean";
    public const string Forest = "forest";
    public const string Sunset = "sunset";
    public const string Violet = "violet";
    public const string Rose = "rose";

    public static readonly string[] All =
    [
        Default,
        Ocean,
        Forest,
        Sunset,
        Violet,
        Rose
    ];

    public static bool IsAllowed(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        return All.Contains(raw.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Default;
        var s = raw.Trim();
        return All.FirstOrDefault(a => a.Equals(s, StringComparison.OrdinalIgnoreCase)) ?? Default;
    }
}
