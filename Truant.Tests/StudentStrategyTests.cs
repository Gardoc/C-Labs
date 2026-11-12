using Truant.Models;
using Truant.Strategies;

namespace Truant.Tests;

public class StudentStrategyTests
{
    [Fact]
    public void Decide_WhenHistoryIsEmpty_AttendsAllSubjects()
    {
        //Arrange
        StudentStrategy strategy = new StudentStrategy();
        List<DayResult> history = new List<DayResult>();

        //Act
        Dictionary<Subject, bool> decisions = strategy.Decide(history);

        //Assert
        Assert.Equal(Enum.GetValues<Subject>().Length, decisions.Count());
        foreach (Subject subject in Enum.GetValues<Subject>())
        {
            Assert.True(decisions.ContainsKey(subject));
            Assert.True(decisions[subject]);
        }
    }

    [Fact]
    public void Decide_WhenConfidenceIsLow_AttendsAllSubjects()
    {
        //Arrange
        FakeTeacherPredictor predictor = new FakeTeacherPredictor(0.1, 0.2);
        StudentStrategy strategy = new StudentStrategy(predictor);
        List<DayResult> history = new List<DayResult>()
        {
            new DayResult(new Dictionary<Subject, bool>())
        };

        //Act
        Dictionary<Subject, bool> decisions = strategy.Decide(history);

        //Assert
        Assert.All(decisions.Values, shouldAttend => Assert.True(shouldAttend));
    }

    [Fact]
    public void Decide_WhenConfidenceIsHighAndAskProbabilityIsLow_SkipsSubject()
    {
        //Arrange
        FakeTeacherPredictor predictor = new FakeTeacherPredictor(0.1, 0.9);
        StudentStrategy strategy = new StudentStrategy(predictor);
        List<DayResult> history = new List<DayResult>()
        {
            new DayResult(new Dictionary<Subject, bool>())
        };

        //Act
        Dictionary<Subject, bool> decisions = strategy.Decide(history);

        //Assert
        Assert.False(decisions[Subject.Calculus]);
    }

    [Fact]
    public void Decide_WhenConfidenceIsHighAndAskProbabilityIsHigh_AttendsSubject()
    {
        //Arrange
        FakeTeacherPredictor predictor = new FakeTeacherPredictor(0.9, 0.9);
        StudentStrategy strategy = new StudentStrategy(predictor);
        List<DayResult> history = new List<DayResult>()
        {
            new DayResult(new Dictionary<Subject, bool>())
        };

        //Act
        Dictionary<Subject, bool> decisions = strategy.Decide(history);

        //Assert
        Assert.True(decisions[Subject.Calculus]);
    }

    [Fact]
    public void Decide_WhenConfidenceEqualsThreshold_UsesExpectedChange() 
    {
        //Arrange
        FakeTeacherPredictor predictor = new FakeTeacherPredictor(0.1, 0.45);
        StudentStrategy strategy = new StudentStrategy(predictor);
        List<DayResult> history = new List<DayResult>()
        {
            new DayResult(new Dictionary<Subject, bool>())
        };

        //Act
        Dictionary<Subject, bool> decisions = strategy.Decide(history);

        //Assert
        Assert.False(decisions[Subject.Calculus]);
    }

    [Fact]
    public void Decide_WhenSubjectsHaveDifferentPredictions_MakesDifferentDecisions()
    {
        //Arrange
        Dictionary<Subject, PredictionResult> predictions = new Dictionary<Subject, PredictionResult>() 
        {
            [Subject.Calculus] = new PredictionResult(0.1, 0.9),
            [Subject.English] = new PredictionResult(0.9, 0.9),            
        };
        FakeTeacherPredictor predictor = new FakeTeacherPredictor(predictions);
        StudentStrategy strategy = new StudentStrategy(predictor);
        List<DayResult> history = new List<DayResult>()
        {
            new DayResult(new Dictionary<Subject, bool>())
        };

        //Act
        Dictionary<Subject, bool> decisions = strategy.Decide(history);

        //Assert
        Assert.False(decisions[Subject.Calculus]);
        Assert.True(decisions[Subject.English]);
    }
}