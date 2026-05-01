namespace InterviewScheduling.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "interview-scheduling";
    public string Audience { get; set; } = "interview-scheduling-clients";
    public string SigningKey { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 120;
}
