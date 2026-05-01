namespace InterviewScheduling.Api.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Comma-separated list of allowed email domains (e.g. contoso.edu, school.org).
    /// When empty, any Google-verified email address is allowed (typical for schools that only use personal Gmail).
    /// </summary>
    public string AllowedEmailDomains { get; set; } = "";

    /// <summary>
    /// When <see cref="AllowedEmailDomains"/> is non-empty, also allow consumer Google Mail addresses
    /// (@gmail.com, @googlemail.com) in addition to the listed domains (mixed Workspace + Gmail schools).
    /// </summary>
    public bool AllowPersonalGoogleEmails { get; set; }

    /// <summary>
    /// Comma-separated institutional emails that should receive the Teacher role on first sign-in.
    /// </summary>
    public string TeacherBootstrapEmails { get; set; } = "";
}
