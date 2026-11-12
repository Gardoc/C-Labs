using Truant.Models;
using Truant.Strategies;

namespace Truant.Tests;

public class TeacherPredictorTests
{
    [Fact]
    public void Predict_WhenHistoryIsEmpty_ReturnsInitialPrediction()
    {
        //Arrange
        TeacherPredictor predictor = new TeacherPredictor();
        List<DayResult> history = new List<DayResult>();

        //Act
        PredictionResult result = predictor.Predict(Subject.Calculus, history);

        //Assert
        Assert.Equal(1.0 / 6.0, result.AskProbability, 5);
        Assert.Equal(1.0 / 3.0, result.Confidence, 5);
    }

    [Fact]
    public void Predict_WhenHistoryFollowsAskIfAskedYesterday_ReturnsValidPrediction()
    {
        // Arrange
        TeacherPredictor predictor = new TeacherPredictor();

        List<DayResult> history = new List<DayResult>
        {
            new DayResult(new Dictionary<Subject, bool>
            {
                [Subject.Calculus] = false,
                [Subject.LinearAlgebra] = false,
                [Subject.DiscreteMath] = false,
                [Subject.Programming] = false,
                [Subject.Physics] = false,
                [Subject.English] = false
            }),
            new DayResult(new Dictionary<Subject, bool>
            {
                [Subject.Calculus] = false,
                [Subject.LinearAlgebra] = false,
                [Subject.DiscreteMath] = false,
                [Subject.Programming] = false,
                [Subject.Physics] = true,
                [Subject.English] = false
            }),
            new DayResult(new Dictionary<Subject, bool>
            {
                [Subject.Calculus] = true,
                [Subject.LinearAlgebra] = false,
                [Subject.DiscreteMath] = false,
                [Subject.Programming] = false,
                [Subject.Physics] = false,
                [Subject.English] = false
            }),
            new DayResult(new Dictionary<Subject, bool>
            {
                [Subject.Calculus] = false,
                [Subject.LinearAlgebra] = false,
                [Subject.DiscreteMath] = false,
                [Subject.Programming] = false,
                [Subject.Physics] = true,
                [Subject.English] = false
            })
        };

        // Act
        PredictionResult result = predictor.Predict(Subject.Calculus, history);

        // Assert
        Assert.InRange(result.AskProbability, 0.0, 1.0);
        Assert.InRange(result.Confidence, 0.0, 1.0);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public void Predict_WhenOnlyRandomRuleMatches_ReturnsHalfProbability()
    {
        // Arrange
        TeacherPredictor predictor = new TeacherPredictor();

        List<DayResult> history = new List<DayResult>
        {
            new DayResult(new Dictionary<Subject, bool>
            {
                [Subject.Calculus] = false,
                [Subject.LinearAlgebra] = false,
                [Subject.DiscreteMath] = false,
                [Subject.Programming] = false,
                [Subject.Physics] = false,
                [Subject.English] = false
            }),
            new DayResult(new Dictionary<Subject, bool>
            {
                [Subject.Calculus] = true,
                [Subject.LinearAlgebra] = true,
                [Subject.DiscreteMath] = true,
                [Subject.Programming] = true,
                [Subject.Physics] = true,
                [Subject.English] = true
            })
        };

        // Act
        PredictionResult result = predictor.Predict(Subject.Calculus, history);

        // Assert
        Assert.Equal(0.5, result.AskProbability, 5);
    }
}
