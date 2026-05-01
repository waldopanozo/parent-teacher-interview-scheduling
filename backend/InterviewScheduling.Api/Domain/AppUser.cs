namespace InterviewScheduling.Api.Domain;

public sealed class AppUser
{
    public Guid Id { get; set; }
    public string GoogleSub { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public AppRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
