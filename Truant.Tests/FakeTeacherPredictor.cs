using Truant.Models;
using Truant.Strategies;

namespace Truant.Tests;
//Поддельный предсказатель, который нужен для тестирования StudentStrategy
public class FakeTeacherPredictor : ITeacherPredictor
{
    private readonly double askProbability;
    private readonly double confidence;
    private readonly Dictionary<Subject, PredictionResult>? predictions;

    public FakeTeacherPredictor(double askProbability, double confidence)
    {
        this.askProbability = askProbability;
        this.confidence = confidence;
    }

    public FakeTeacherPredictor(Dictionary<Subject, PredictionResult> predictions)
    {
        this.predictions = predictions; 
    }

    public PredictionResult Predict(Subject subject, List<DayResult> history)
    {
        if(predictions != null && predictions.TryGetValue(subject, out PredictionResult? prediction))
        {
            return prediction;
        }
        return new PredictionResult(askProbability, confidence);
    }
}
