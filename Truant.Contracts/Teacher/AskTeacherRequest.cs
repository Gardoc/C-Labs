namespace Truant.Contracts.Teacher;
public class AskTeacherRequest
{
    public long SemesterId { get; set; }
    public int DayNumber { get; set; }
    public double? RandomValue { get; set; }
}