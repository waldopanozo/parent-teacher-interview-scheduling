namespace InterviewScheduling.Api.Domain;

/// <summary>
/// A teacher may expose multiple offerings: same or different subjects, courses, or grade levels.
/// </summary>
public sealed class TeacherOffering
{
    public Guid Id { get; set; }
    public Guid TeacherUserId { get; set; }
    public AppUser Teacher { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public string CourseTitle { get; set; } = "";
    public string GradeLevel { get; set; } = "";
    public ICollection<WeeklyAvailability> WeeklyAvailabilities { get; set; } = new List<WeeklyAvailability>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
