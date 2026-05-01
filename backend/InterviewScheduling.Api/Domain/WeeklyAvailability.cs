namespace InterviewScheduling.Api.Domain;

/// <summary>
/// Recurring weekly window in the configured school time zone (local wall clock).
/// </summary>
public sealed class WeeklyAvailability
{
    public Guid Id { get; set; }
    public Guid TeacherOfferingId { get; set; }
    public TeacherOffering TeacherOffering { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartLocal { get; set; }
    public TimeSpan EndLocal { get; set; }
}
