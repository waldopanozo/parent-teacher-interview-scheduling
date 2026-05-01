namespace InterviewScheduling.Api.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Comma-separated list of allowed email domains (e.g. contoso.edu, school.org).
    /// </summary>
    public string AllowedEmailDomains { get; set; } = "";

    /// <summary>
    /// Comma-separated institutional emails that should receive the Teacher role on first sign-in.
    /// </summary>
    public string TeacherBootstrapEmails { get; set; } = "";
}
