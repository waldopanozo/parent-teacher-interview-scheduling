using System.Text.Json.Serialization;

namespace InterviewScheduling.Api.Contracts;

public sealed class GoogleSignInRequest
{
    public string IdToken { get; set; } = "";
}

public sealed class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public sealed class EmailLoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class CreateTeacherOfferingRequest
{
    public Guid SubjectId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string GradeLevel { get; set; } = "";
    public string SectionLabel { get; set; } = "";
}

public sealed class DirectorCreateTeacherOfferingRequest
{
    public Guid TeacherUserId { get; set; }
    public Guid SubjectId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string GradeLevel { get; set; } = "";
    public string SectionLabel { get; set; } = "";
}

public sealed class SubmitTeacherAccessRequest
{
    public string? Message { get; set; }
}

public sealed class CreateSubjectRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class UpdateSubjectRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class ReplaceWeeklyAvailabilityRequest
{
    public List<WeeklyAvailabilityItemRequest> Windows { get; set; } = new();
}

public sealed class WeeklyAvailabilityItemRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Local wall-clock start, HH:mm (24h).</summary>
    public string StartLocal { get; set; } = "";

    /// <summary>Local wall-clock end, HH:mm (24h).</summary>
    public string EndLocal { get; set; } = "";
}

public sealed class CreateBookingRequest
{
    public Guid TeacherOfferingId { get; set; }
    public DateTime StartUtc { get; set; }
}

public sealed class UpsertParentMeetingProfileRequest
{
    /// <summary>Student school or institutional email (parent may use a different Google account).</summary>
    public string StudentSchoolEmail { get; set; } = "";

    /// <summary>Full name of the person who will attend the interview.</summary>
    public string InterviewAttendeeName { get; set; } = "";

    /// <summary>Relationship to the student (e.g. mother, father, guardian).</summary>
    public string RelationshipToStudent { get; set; } = "";
}
