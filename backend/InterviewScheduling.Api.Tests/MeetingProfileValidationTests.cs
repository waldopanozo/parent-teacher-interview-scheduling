using InterviewScheduling.Api.Domain;
using InterviewScheduling.Api.Services;
using Xunit;

namespace InterviewScheduling.Api.Tests;

public sealed class MeetingProfileValidationTests
{
    [Fact]
    public void ValidateForSave_rejects_empty_fields()
    {
        var err = MeetingProfileValidation.ValidateForSave("", "Jane Doe", "Mother");
        Assert.NotNull(err);
    }

    [Fact]
    public void ValidateForSave_rejects_invalid_email()
    {
        var err = MeetingProfileValidation.ValidateForSave("not-an-email", "Jane Doe", "Mother");
        Assert.NotNull(err);
    }

    [Fact]
    public void ValidateForSave_accepts_valid_row()
    {
        var err = MeetingProfileValidation.ValidateForSave("student@school.edu", "Jane Doe", "Mother");
        Assert.Null(err);
    }

    [Fact]
    public void IsComplete_parent_without_fields_is_false()
    {
        var u = new AppUser { Role = AppRole.Parent };
        Assert.False(MeetingProfileValidation.IsComplete(u));
    }

    [Fact]
    public void IsComplete_parent_with_all_fields_is_true()
    {
        var u = new AppUser
        {
            Role = AppRole.Parent,
            StudentSchoolEmail = "student@school.edu",
            InterviewAttendeeName = "Jane Doe",
            RelationshipToStudent = "Mother"
        };
        Assert.True(MeetingProfileValidation.IsComplete(u));
    }

    [Fact]
    public void IsComplete_teacher_always_true()
    {
        var u = new AppUser { Role = AppRole.Teacher };
        Assert.True(MeetingProfileValidation.IsComplete(u));
    }
}
