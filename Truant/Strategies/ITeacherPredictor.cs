using Truant.Models;

namespace Truant.Strategies;

public interface ITeacherPredictor
{
    PredictionResult Predict(Subject subject, List<DayResult> history); 
}
