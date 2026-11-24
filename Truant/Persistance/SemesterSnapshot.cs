using Truant.Models;

namespace Truant.Persistance;

public class SemesterSnapshot
{
    public long Id { get; set; }

    public int CurrentDay { get; set; }

    public long RandomState { get; set; }

    public string Status { get; set; } = "in_progress";

    public StudentState Student { get; set; } = new();

    public List<Teacher> Teachers { get; set; } = new();

    public List<DayResult> History { get; set; } = new();
}
