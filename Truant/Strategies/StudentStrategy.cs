using Truant.Models;

namespace Truant.Strategies;

internal class StudentStrategy
{
    private readonly TeacherPredictor predictor;
    private double epsylon = 0.45;
    private double expectedEpsylon = 0;

    public StudentStrategy()
    {
        predictor = new TeacherPredictor();
    }

    public Dictionary<Subject, bool> Decide(List<DayResult> history, double pleasure)
    {
        var decisions = new Dictionary<Subject, bool>();
        foreach (Subject subject in Enum.GetValues<Subject>())
        {
            decisions[subject] = ShouldAttend(subject, history, pleasure);
        }

        return decisions;

    }

    private bool ShouldAttend(Subject subject, List<DayResult> history, double pleasure)
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

        double expectedChange = (1 - askprobability) - askprobability * pleasure; // (вероятность, что не спросят) - награда за прогул

        if (expectedChange > expectedEpsylon)
        {
            return false;
        }

        return true;

    }
}