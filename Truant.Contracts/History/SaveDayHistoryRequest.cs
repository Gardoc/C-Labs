namespace Truant.Contracts.History;

public class SaveDayHistoryRequest
{
    public long SemesterId { get; set; }
    public int DayNumber { get; set; }
    public List<SubjectAskResultDto> Results { get; set; } = new();
}
