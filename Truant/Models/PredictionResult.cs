using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truant.Models
{
    internal class PredictionResult
    {
        public double AskProbability { get;}
        public double Confidence { get;}

        public PredictionResult(double askProbability, double confidence)
        {
            AskProbability = askProbability;
            Confidence = confidence;
        }
    }
}
