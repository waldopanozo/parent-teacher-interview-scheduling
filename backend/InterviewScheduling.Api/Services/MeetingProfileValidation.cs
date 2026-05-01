using System.Net.Mail;
using InterviewScheduling.Api.Domain;

namespace InterviewScheduling.Api.Services;

/// <summary>
/// Validates parent meeting profile fields used for interview bookings.
/// </summary>
public static class MeetingProfileValidation
{
    public static bool IsComplete(AppUser parent)
    {
        if (parent.Role != AppRole.Parent)
            return true;
        return !string.IsNullOrWhiteSpace(parent.StudentSchoolEmail)
               && !string.IsNullOrWhiteSpace(parent.InterviewAttendeeName)
               && !string.IsNullOrWhiteSpace(parent.RelationshipToStudent);
    }

    /// <summary>Returns null if valid; otherwise a short error message.</summary>
    public static string? ValidateForSave(string studentSchoolEmail, string interviewAttendeeName,
        string relationshipToStudent)
    {
        var se = studentSchoolEmail.Trim();
        var an = interviewAttendeeName.Trim();
        var rel = relationshipToStudent.Trim();

        if (se.Length == 0 || an.Length == 0 || rel.Length == 0)
            return "studentSchoolEmail, interviewAttendeeName, and relationshipToStudent are required.";

        if (se.Length > 320 || an.Length > 200 || rel.Length > 120)
            return "One or more fields exceed maximum length.";

        if (!LooksLikeEmail(se))
            return "studentSchoolEmail must look like a valid email address.";

        return null;
    }

    private static bool LooksLikeEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
