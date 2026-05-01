namespace InterviewScheduling.Api.Contracts;

public sealed class ParentMeetingProfileDto(string? studentSchoolEmail, string? interviewAttendeeName,
    string? relationshipToStudent)
{
    public string? StudentSchoolEmail { get; } = studentSchoolEmail;
    public string? InterviewAttendeeName { get; } = interviewAttendeeName;
    public string? RelationshipToStudent { get; } = relationshipToStudent;
}
