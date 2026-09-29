using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truant.Models;

namespace Truant.Rules
{
    internal static class TeacherRuleLogic
    {
        public static bool ShouldAsk(Teacher teacher, Dictionary<Subject, bool> yesterday, Random rnd)
        {
            switch(teacher.Rule){
                case TeacherRule.Random:
                    return rnd.NextDouble() < 0.5;
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
}
