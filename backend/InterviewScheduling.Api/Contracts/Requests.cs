using System.Text.Json.Serialization;

namespace InterviewScheduling.Api.Contracts;

public sealed class GoogleSignInRequest
{
    public string IdToken { get; set; } = "";
}

public sealed class CreateTeacherOfferingRequest
{
    public Guid SubjectId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string GradeLevel { get; set; } = "";
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
