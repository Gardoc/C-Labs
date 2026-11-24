namespace Truant.Teacher.Service.Models;
public class TeacherConfiguration
{
    public long SemesterId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int Rule { get; set; }
    public string? SubjectA { get; set; }
    public string? SubjectB { get; set; }
}