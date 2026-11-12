using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truant.Models;

public class SimulationStartupOptions
{
    public bool ResumeExisting { get; set;}

    public long? SemesterId { get; set;}
}
