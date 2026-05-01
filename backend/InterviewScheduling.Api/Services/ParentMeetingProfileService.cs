using InterviewScheduling.Api.Contracts;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewScheduling.Api.Services;

public sealed class ParentMeetingProfileService(AppDbContext db)
{
    public async Task<ParentMeetingProfileDto?> GetAsync(Guid userId, CancellationToken ct)
    {
        var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (u is null || u.Role != AppRole.Parent)
            return null;
        return new ParentMeetingProfileDto(u.StudentSchoolEmail, u.InterviewAttendeeName, u.RelationshipToStudent);
    }

    public async Task<(bool Ok, string? Error)> UpsertAsync(Guid userId, UpsertParentMeetingProfileRequest body,
        CancellationToken ct)
    {
        var err = MeetingProfileValidation.ValidateForSave(body.StudentSchoolEmail, body.InterviewAttendeeName,
            body.RelationshipToStudent);
        if (err is not null)
            return (false, err);

        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (u is null || u.Role != AppRole.Parent)
            return (false, "Only parent accounts can save a meeting profile.");

        u.StudentSchoolEmail = body.StudentSchoolEmail.Trim();
        u.InterviewAttendeeName = body.InterviewAttendeeName.Trim();
        u.RelationshipToStudent = body.RelationshipToStudent.Trim();
        await db.SaveChangesAsync(ct);
        return (true, null);
    }
}
