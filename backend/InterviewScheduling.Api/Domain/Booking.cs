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

    /// <summary>Copy of student identifier at booking time.</summary>
    public string StudentSchoolEmail { get; set; } = "";

    /// <summary>Copy of attendee name at booking time.</summary>
    public string InterviewAttendeeName { get; set; } = "";

    /// <summary>Copy of relationship at booking time.</summary>
    public string RelationshipToStudent { get; set; } = "";

    /// <summary>When set, the parent cancelled online; row kept for audit.</summary>
    public DateTimeOffset? CancelledAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    /// <summary>Set by the teacher after the slot (not for cancelled rows).</summary>
    public AttendanceStatus AttendanceStatus { get; set; }

    /// <summary>Teacher notes on the conversation / visit.</summary>
    public string? VisitNotes { get; set; }
}
