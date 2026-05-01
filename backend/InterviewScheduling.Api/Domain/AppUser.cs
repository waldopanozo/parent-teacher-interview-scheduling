namespace InterviewScheduling.Api.Domain;

public sealed class AppUser
{
    public Guid Id { get; set; }
    public string GoogleSub { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public AppRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>School or institutional email identifying the student (parent uses their own Google account).</summary>
    public string? StudentSchoolEmail { get; set; }

    /// <summary>Legal name of the adult who will attend the interview.</summary>
    public string? InterviewAttendeeName { get; set; }

    /// <summary>Kinship to the student (e.g. mother, father, legal guardian).</summary>
    public string? RelationshipToStudent { get; set; }
}
