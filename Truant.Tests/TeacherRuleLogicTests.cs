using Truant.Models;
using Truant.Rules;

namespace Truant.Tests;

public class TeacherRuleLogicTests
{
    [Fact]
    public void ShouldAsk_WhenRandomRuleAndRandomIsZero_ReturnsTrue()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.Random);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>();

        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.2);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldAsk_WhenAskedYesterdayRuleAndSubjectWasAsked_ReturnsTrue()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.IfAskedYesterday, Subject.Physics);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>() {[Subject.Physics] = true };
        
        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.5);
        
        //Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldAsk_WhenAskedHistoryMatchesRuleAndSubjectWasAsked_ReturnsTrue()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.IfAskedHistory, Subject.Physics, Subject.English);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>() { [Subject.Physics] = true, [Subject.English] = false };

        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.5);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldAsk_WhenRandomValueIsAboveThreshold_ReturnsFalse()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.Random);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>();

        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.8);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldAsk_WhenAskedYesterdayRuleAndSubjectWasAsked_ReturnsFalse()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.IfAskedYesterday, Subject.Physics);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>() { [Subject.Physics] = false };

        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.5);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldAsk_WhenAskedHistoryMatchesRuleAndSubjectWasAsked_ReturnsFalse()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.IfAskedHistory, Subject.Physics, Subject.English);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>() { [Subject.Physics] = true, [Subject.English] = true };

        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.5);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldAsk_WhenBothHistoryResultsAreFalse_ReturnsFalse()
    {
        //Arrange
        Teacher teacher = new Teacher(Subject.Calculus, TeacherRule.IfAskedHistory, Subject.Physics, Subject.English);
        Dictionary<Subject, bool> yesterday = new Dictionary<Subject, bool>() { [Subject.Physics] = false, [Subject.English] = false };

        //Act
        bool result = TeacherRuleLogic.ShouldAsk(teacher, yesterday, () => 0.5);

        //Assert
        Assert.False(result);
    }
}
