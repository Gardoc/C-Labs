namespace Truant.Contracts.Teacher;
public class AskTeacherResponse
{
    public string Subject { get; set; } = string.Empty;
    public bool WasAsked { get; set; }
}