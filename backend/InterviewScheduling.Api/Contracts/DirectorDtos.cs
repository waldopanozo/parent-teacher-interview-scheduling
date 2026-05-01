using InterviewScheduling.Api.Domain;

namespace InterviewScheduling.Api.Contracts;

public sealed record TeacherListItemDto(Guid Id, string Email, string DisplayName);

public sealed class TeacherAccessRequestListItemDto(
    Guid id,
    Guid applicantUserId,
    string applicantEmail,
    string applicantDisplayName,
    string? message,
    TeacherAccessRequestStatus status,
    DateTimeOffset createdAt)
{
    public Guid Id { get; } = id;
    public Guid ApplicantUserId { get; } = applicantUserId;
    public string ApplicantEmail { get; } = applicantEmail;
    public string ApplicantDisplayName { get; } = applicantDisplayName;
    public string? Message { get; } = message;
    public TeacherAccessRequestStatus Status { get; } = status;
    public DateTimeOffset CreatedAt { get; } = createdAt;
}
