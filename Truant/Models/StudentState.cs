using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truant.Models
{
    internal class StudentState
    {
        public bool IsExpelled {  get; set; }   
        public double Pleasure {  get; set; }  
        public int MissedLessons { get; set; }
        public int DaysCompleted { get; set; }


        public StudentState() { 
            IsExpelled = false;
            Pleasure = 0;
            MissedLessons = 0;
            DaysCompleted = 0;
        }
    }
}
