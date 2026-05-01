namespace InterviewScheduling.Api.Domain;

public sealed class Subject
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}
