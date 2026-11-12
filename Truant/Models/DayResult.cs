namespace Truant.Models;

public class DayResult
{
    public Dictionary<Subject, bool> Asked { get; }

    public DayResult(Dictionary<Subject, bool> asked)
    {
        Asked = asked;       
    }
}
