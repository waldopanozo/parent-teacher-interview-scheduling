namespace InterviewScheduling.Api.Domain;

public sealed class Booking
{
    public Guid Id { get; set; }
    public Guid TeacherOfferingId { get; set; }
    public TeacherOffering TeacherOffering { get; set; } = null!;
    public Guid ParentUserId { get; set; }
    public AppUser Parent { get; set; } = null!;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
