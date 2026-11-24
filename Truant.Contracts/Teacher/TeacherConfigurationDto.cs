namespace Truant.Contracts.Teacher;
public class TeacherConfigurationDto
{
    public string Subject { get; set; } = string.Empty;
    public int Rule { get; set; }
    public string? SubjectA { get; set; }
    public string? SubjectB { get; set; }
}