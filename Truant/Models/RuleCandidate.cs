namespace Truant.Models;

internal class RuleCandidate
{
    public TeacherRule Rule { get; }
    public Subject? SubjectA { get; }
    public Subject? SubjectB { get; }

    public RuleCandidate(TeacherRule rule, Subject? subjectA = null, Subject? subjectB = null)
    {      
        Rule = rule;
        SubjectA = subjectA;
        SubjectB = subjectB;
    }
}
