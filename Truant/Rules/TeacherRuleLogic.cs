using Truant.Models;

namespace Truant.Rules;

public static class TeacherRuleLogic
{
    public static bool ShouldAsk(Teacher teacher, Dictionary<Subject, bool> yesterday, Func<double> nextRandom)
    {
        switch (teacher.Rule){
            case TeacherRule.Random:
                return nextRandom() < 0.5;
            case TeacherRule.IfAskedYesterday:
                return yesterday[teacher.SubjectA!.Value];
            case TeacherRule.IfAskedHistory:
                bool askedA = yesterday[teacher.SubjectA!.Value];
                bool askedB = yesterday[teacher.SubjectB!.Value];
                return askedA != askedB;

            default:
                throw new ArgumentOutOfRangeException("Argument out of range");
        }
    }
}
