namespace InterviewScheduling.Api.Domain;

public sealed class TeacherAccessRequest
{
    public Guid Id { get; set; }
    public Guid ApplicantUserId { get; set; }
    public AppUser Applicant { get; set; } = null!;
    public string? Message { get; set; }
    public TeacherAccessRequestStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public AppUser? DecidedBy { get; set; }
}
