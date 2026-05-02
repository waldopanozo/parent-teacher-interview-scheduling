using InterviewScheduling.Api.Domain;

namespace InterviewScheduling.Api.Contracts;

public sealed class TeacherOfferingSummaryDto(
    Guid id,
    string teacherDisplayName,
    string subjectName,
    string subjectCode,
    string courseTitle,
    string gradeLevel,
    string sectionLabel)
{
    public Guid Id { get; } = id;
    public string TeacherDisplayName { get; } = teacherDisplayName;
    public string SubjectName { get; } = subjectName;
    public string SubjectCode { get; } = subjectCode;
    public string CourseTitle { get; } = courseTitle;
    public string GradeLevel { get; } = gradeLevel;
    public string SectionLabel { get; } = sectionLabel;
}

public sealed class SubjectSummaryDto(Guid id, string code, string name)
{
    public Guid Id { get; } = id;
    public string Code { get; } = code;
    public string Name { get; } = name;
}

public sealed class AuthResponseDto(string accessToken, DateTime expiresAtUtc, UserProfileDto user)
{
    public string AccessToken { get; } = accessToken;
    public DateTime ExpiresAtUtc { get; } = expiresAtUtc;
    public UserProfileDto User { get; } = user;
}

public sealed class UserProfileDto(Guid id, string email, string displayName, AppRole role, bool meetingProfileComplete)
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
    public string DisplayName { get; } = displayName;
    public AppRole Role { get; } = role;

    /// <summary>For parents: all meeting fields saved. Always true for Teacher/Director.</summary>
    public bool MeetingProfileComplete { get; } = meetingProfileComplete;
}

public sealed class SchoolSettingsResponseDto(string schoolTimeZoneId, DateTimeOffset? updatedAt, Guid? updatedByDirectorId)
{
    public string SchoolTimeZoneId { get; } = schoolTimeZoneId;
    public DateTimeOffset? UpdatedAt { get; } = updatedAt;
    public Guid? UpdatedByDirectorId { get; } = updatedByDirectorId;
}

public sealed class CancelledBookingAuditDto(
    Guid id,
    DateTimeOffset? cancelledAt,
    Guid? cancelledByUserId,
    DateTime startUtc,
    DateTime endUtc,
    string parentEmail,
    string parentDisplayName,
    string teacherDisplayName,
    string subjectName,
    string courseTitle,
    string gradeLevel,
    string sectionLabel)
{
    public Guid Id { get; } = id;
    public DateTimeOffset? CancelledAt { get; } = cancelledAt;
    public Guid? CancelledByUserId { get; } = cancelledByUserId;
    public DateTime StartUtc { get; } = startUtc;
    public DateTime EndUtc { get; } = endUtc;
    public string ParentEmail { get; } = parentEmail;
    public string ParentDisplayName { get; } = parentDisplayName;
    public string TeacherDisplayName { get; } = teacherDisplayName;
    public string SubjectName { get; } = subjectName;
    public string CourseTitle { get; } = courseTitle;
    public string GradeLevel { get; } = gradeLevel;
    public string SectionLabel { get; } = sectionLabel;
}
