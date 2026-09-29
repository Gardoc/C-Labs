using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Truant.Models
{
    internal class Teacher
    {
        public Subject Subject {get;}       
        public TeacherRule Rule {get;}

        public Subject? SubjectA {get;}
        public Subject? SubjectB {get;}

        public Teacher(Subject subject, TeacherRule rule, Subject? subjectA = null, Subject? subjectB = null)
        {
            Subject = subject;
            Rule = rule;
            SubjectA = subjectA;
            SubjectB = subjectB;
        }
        
    }
}
