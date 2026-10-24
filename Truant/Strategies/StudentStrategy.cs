using Truant.Models;

namespace Truant.Strategies;

public class StudentStrategy
{
    private readonly ITeacherPredictor predictor;
    private double epsylon = 0.45;
    private double expectedEpsylon = 0.1;

    public StudentStrategy() : this(new TeacherPredictor())
    {
        
    }

    public StudentStrategy(ITeacherPredictor predictor)
    {
        this.predictor = predictor;
    }

    public Dictionary<Subject, bool> Decide(List<DayResult> history)
    {
        var decisions = new Dictionary<Subject, bool>();
        foreach (Subject subject in Enum.GetValues<Subject>())
        {
            decisions[subject] = ShouldAttend(subject, history);
        }

        return decisions;

    }

    private bool ShouldAttend(Subject subject, List<DayResult> history)
    {
        if (history.Count == 0)
        {
            return true;
        }

        PredictionResult prediction = predictor.Predict(subject, history);
        double askprobability = prediction.AskProbability;
        double confidence = prediction.Confidence;
        if (confidence < epsylon)
        {
            return true;
        }

        return askprobability > expectedEpsylon;

    }
}