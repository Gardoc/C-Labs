using Truant.Models;
using Truant.Simulation;
using Truant.Strategies;
using Xunit;

namespace Truant.Tests.IntegrationTests;

[Collection("Database collection")]
public class SemesterRepositoryTests : IAsyncLifetime
{
    private readonly TestDatabase fixture;

    public SemesterRepositoryTests()
    {
        fixture = new TestDatabase();
    }

    // Выполняется перед каждым тестом
    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
    }

    // Выполняется после каждого теста
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateSemesterAsync_ShouldSaveSemesterToDatabase()
    {
        // Arrange
        var student = new StudentState
        {
            Pleasure = 15,
            MissedLessons = 3,
            DaysCompleted = 5,
            IsExpelled = false
        };

        int currentDay = 5;
        long randomState = 123456789;

        // Act
        long semesterId = await fixture.Repository.CreateSemesterAsync(student, currentDay, randomState);
        var snapshot = await fixture.Repository.GetSemesterAsync(semesterId);

        // Assert
        Assert.NotNull(snapshot);

        Assert.Equal(semesterId, snapshot.Id);
        Assert.Equal(currentDay, snapshot.CurrentDay);
        Assert.Equal(student.Pleasure, snapshot.Student.Pleasure);
        Assert.Equal(student.MissedLessons, snapshot.Student.MissedLessons);
        Assert.Equal(student.DaysCompleted, snapshot.Student.DaysCompleted);
        Assert.Equal(student.IsExpelled, snapshot.Student.IsExpelled);
        Assert.Equal(randomState, snapshot.RandomState);
        Assert.Equal("in_progress", snapshot.Status);
    }

    [Fact]
    public async Task SaveTeachersAsync_ShouldSaveAllTeachersAndTheirRules()
    {
        // Arrange
        var student = new StudentState();

        long semesterId = await fixture.Repository.CreateSemesterAsync(student, currentDay: 0, randomState: 123456789);

        var teachers = new List<Teacher>
        {
            new(Subject.Calculus, TeacherRule.Random),
            new(Subject.LinearAlgebra, TeacherRule.IfAskedYesterday, Subject.Calculus),
            new(Subject.DiscreteMath, TeacherRule.IfAskedHistory, Subject.Calculus, Subject.English),
            new(Subject.Programming, TeacherRule.Random),
            new(Subject.Physics, TeacherRule.IfAskedYesterday, Subject.Programming),
            new(Subject.English, TeacherRule.IfAskedHistory, Subject.Physics, Subject.LinearAlgebra)
        };

        // Act
        await fixture.Repository.SaveTeachersAsync(semesterId, teachers);
        var loadedTeachers = await fixture.Repository.GetTeachersAsync(semesterId);

        // Assert
        Assert.Equal(6, loadedTeachers.Count);

        foreach (var original in teachers)
        {
            var loaded = Assert.Single(loadedTeachers.Where(t => t.Subject == original.Subject));

            Assert.Equal(original.Rule, loaded.Rule);
            Assert.Equal(original.SubjectA, loaded.SubjectA);
            Assert.Equal(original.SubjectB, loaded.SubjectB);
        }
    }

    [Fact]
    public async Task SaveDayAsync_ShouldSaveStudentStateAndDayResults()
    {
        // Arrange
        var student = new StudentState();

        long semesterId = await fixture.Repository.CreateSemesterAsync(student, currentDay: 0, randomState: 123456789);

        var teachers = new List<Teacher>
        {
            new(Subject.Calculus, TeacherRule.Random),
            new(Subject.LinearAlgebra, TeacherRule.Random),
            new(Subject.DiscreteMath, TeacherRule.Random),
            new(Subject.Programming, TeacherRule.Random),
            new(Subject.Physics, TeacherRule.Random),
            new(Subject.English, TeacherRule.Random)
        };

        await fixture.Repository.SaveTeachersAsync(semesterId, teachers);

        student.Pleasure = 2;
        student.MissedLessons = 1;
        student.DaysCompleted = 1;

        var asked = new Dictionary<Subject, bool>
        {
            [Subject.Calculus] = true,
            [Subject.LinearAlgebra] = false,
            [Subject.DiscreteMath] = true,
            [Subject.Programming] = false,
            [Subject.Physics] = false,
            [Subject.English] = true
        };

        var dayResult = new DayResult(asked);
        long randomState = 987654321;

        // Act
        await fixture.Repository.SaveDayAsync(semesterId, currentDay: 1, student, dayResult, randomState);
        var snapshot = await fixture.Repository.LoadSemesterAsync(semesterId);

        // Assert
        Assert.NotNull(snapshot);

        Assert.Equal(1, snapshot.CurrentDay);
        Assert.Equal(2, snapshot.Student.Pleasure);
        Assert.Equal(1, snapshot.Student.MissedLessons);
        Assert.Equal(1, snapshot.Student.DaysCompleted);
        Assert.Equal(randomState, snapshot.RandomState);

        Assert.Single(snapshot.History);

        foreach (Subject subject in Enum.GetValues<Subject>())
        {
            Assert.Equal(asked[subject], snapshot.History[0].Asked[subject]);
        }
    }

    [Fact]
    public async Task LoadSemesterAsync_ShouldRestoreCompleteSemester()
    {
        // Arrange
        var student = new StudentState
        {
            Pleasure = 5,
            MissedLessons = 2,
            DaysCompleted = 3
        };

        long semesterId = await fixture.Repository.CreateSemesterAsync(student, currentDay: 3, randomState: 123456789);

        var teachers = new List<Teacher>
        {
            new(Subject.Calculus, TeacherRule.Random),
            new(Subject.LinearAlgebra, TeacherRule.Random),
            new(Subject.DiscreteMath, TeacherRule.Random),
            new(Subject.Programming, TeacherRule.Random),
            new(Subject.Physics, TeacherRule.Random),
            new(Subject.English, TeacherRule.Random)
        };

        await fixture.Repository.SaveTeachersAsync(semesterId, teachers);

        var asked = new Dictionary<Subject, bool>
        {
            [Subject.Calculus] = true,
            [Subject.LinearAlgebra] = false,
            [Subject.DiscreteMath] = true,
            [Subject.Programming] = true,
            [Subject.Physics] = false,
            [Subject.English] = false
        };

        await fixture.Repository.SaveDayAsync(semesterId, 3, student, new DayResult(asked), 987654321);

        // Act
        var snapshot = await fixture.Repository.LoadSemesterAsync(semesterId);

        // Assert
        Assert.NotNull(snapshot);

        Assert.Equal(semesterId, snapshot.Id);
        Assert.Equal(3, snapshot.CurrentDay);

        Assert.Equal(5, snapshot.Student.Pleasure);
        Assert.Equal(2, snapshot.Student.MissedLessons);
        Assert.Equal(3, snapshot.Student.DaysCompleted);
        Assert.False(snapshot.Student.IsExpelled);

        Assert.Equal(987654321, snapshot.RandomState);
        Assert.Equal("in_progress", snapshot.Status);

        Assert.Equal(6, snapshot.Teachers.Count);
        Assert.Single(snapshot.History);

        foreach (Subject subject in Enum.GetValues<Subject>())
        {
            Assert.Equal(asked[subject], snapshot.History[0].Asked[subject]);
        }
    }

    [Fact]
    public async Task GetLatestUnfinishedSemesterAsync_ShouldReturnLatestSemester()
    {
        // Arrange
        var student = new StudentState();

        long firstId = await fixture.Repository.CreateSemesterAsync(student, 0, 111);
        long secondId = await fixture.Repository.CreateSemesterAsync(student, 0, 222);

        // Act
        var latest = await fixture.Repository.GetLatestUnfinishedSemesterAsync();

        // Assert
        Assert.NotNull(latest);
        Assert.Equal(secondId, latest.Id);
        Assert.NotEqual(firstId, latest.Id);
    }

    [Fact]
    public async Task GetLatestUnfinishedSemesterAsync_WhenNoSemestersExist_ShouldReturnNull()
    {
        // Act
        var latest = await fixture.Repository.GetLatestUnfinishedSemesterAsync();

        // Assert
        Assert.Null(latest);
    }

    private class AlwaysAttendPredictor : ITeacherPredictor
    {
        public PredictionResult Predict(Subject subject, List<DayResult> history)
        {
            return new PredictionResult(askProbability: 0.5, confidence: 0.2);
        }
    }
}