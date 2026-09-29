using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truant.Models
{
    internal class DayResult
    {
        public Dictionary<Subject, bool> Asked { get; }
        public DayResult(Dictionary<Subject, bool> asked)
        {
            Asked = asked;       
        }


    }
}
