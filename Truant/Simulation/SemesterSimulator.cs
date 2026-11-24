using Truant.Clients;
using Truant.Contracts.History;
using Truant.Contracts.Teacher;
using Truant.Models;
using Truant.Persistance;
using Truant.Strategies;

namespace Truant.Simulation;

public class SemesterSimulator
{
    private RandomGenerator random;
    private List<Teacher> teachers;
    private readonly StudentStrategy strategy;
    private StudentState studentState;
    private List<DayResult> history;
    private readonly SemesterRepository repository;
    private readonly TeacherServiceClient teacherServiceClient;
    private readonly HistoryServiceClient historyServiceClient;

    private int currentDay;
    private long semesterId;

    public SemesterSimulator(RandomGenerator random, StudentStrategy strategy, SemesterRepository repository, TeacherServiceClient teacherServiceClient, HistoryServiceClient historyServiceClient)
    {
        this.random = random;
        this.strategy = strategy;
        this.repository = repository;
        this.teacherServiceClient = teacherServiceClient;
        this.historyServiceClient = historyServiceClient;

        teachers = new List<Teacher>();
        studentState = new StudentState();
        history = new List<DayResult>();
        currentDay = 0;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        teachers = TeacherGenerator.CreateTeachers(random);

        Console.WriteLine("Преподаватели:");

        foreach (Teacher teacher in teachers)
        {
            Console.WriteLine(
                $"{teacher.Subject}: Правило = {teacher.Rule}, " +
                $"A = {teacher.SubjectA}, B = {teacher.SubjectB}");
        }

        Console.WriteLine();

        studentState = new StudentState();
        history = new List<DayResult>();
        currentDay = 0;

        semesterId = await repository.CreateSemesterWithTeachersAsync(studentState, currentDay, random.State, teachers, cancellationToken);

        var configurations = teachers.Select(teacher =>
            new TeacherConfigurationDto
            {
                Subject = teacher.Subject.ToString(),
                Rule = ConvertRuleToInt(teacher.Rule),
                SubjectA = teacher.SubjectA?.ToString(),
                SubjectB = teacher.SubjectB?.ToString()
            }).ToList();

        await teacherServiceClient.SaveConfigurationsAsync(semesterId, configurations, cancellationToken);

        Console.WriteLine($"Создан новый семестр. ID: {semesterId}");
    }

    public async Task ResumeAsync(long semesterId, CancellationToken cancellationToken = default)
    {
        SemesterSnapshot? snapshot = await repository.LoadSemesterAsync(semesterId, cancellationToken);

        if (snapshot == null)
        {
            throw new InvalidOperationException($"Семестр с ID {semesterId} не найден.");
        }

        if (snapshot.Status != "in_progress")
        {
            throw new InvalidOperationException($"Нельзя продолжить семестр со статусом: {snapshot.Status}");
        }

        this.semesterId = snapshot.Id;
        currentDay = snapshot.CurrentDay;

        studentState = snapshot.Student;
        teachers = snapshot.Teachers;
        history = snapshot.History;

        random = new RandomGenerator(unchecked((ulong)snapshot.RandomState));

        Console.WriteLine("Семестр успешно восстановлен!");
        Console.WriteLine($"ID: {semesterId}");
        Console.WriteLine($"Продолжаем с дня: {currentDay + 1}");
        Console.WriteLine($"Преподавателей: {teachers.Count}");
        Console.WriteLine($"Дней в истории: {history.Count}");
        Console.WriteLine();
    }

    public async Task<bool> RunDayAsync(CancellationToken cancellationToken = default)
    {
        currentDay++;

        Console.WriteLine($"День {currentDay}");

        var decisions = strategy.Decide(history);
        var today = new Dictionary<Subject, bool>();

        bool isExpelled = false;

        foreach (Teacher teacher in teachers)
        {
            double? randomValue = teacher.Rule == TeacherRule.Random
                ? random.NextDouble()
                : null;

            var request = new AskTeacherRequest
            {
                SemesterId = semesterId,
                DayNumber = currentDay,
                RandomValue = randomValue
            };

            AskTeacherResponse response = await teacherServiceClient.AskAsync(request, teacher.Subject.ToString(), cancellationToken);

            bool willAsk = response.WasAsked;

            today[teacher.Subject] = willAsk;

            bool willAttend = decisions[teacher.Subject];

            Console.WriteLine(
                $"{teacher.Subject}: " +
                $"студент {(willAttend ? "пришел" : "прогулял")}, " +
                $"преподаватель {(willAsk ? "спросил" : "не спросил")}");

            if (!willAttend)
            {
                studentState.MissedLessons++;
                studentState.Pleasure++;

                if (willAsk)
                {
                    studentState.IsExpelled = true;
                    studentState.Pleasure = 0;
                    isExpelled = true;

                    Console.WriteLine("СТУДЕНТ ВЫЛЕТЕЛ!");
                    break;
                }
            }
        }

        if (!isExpelled)
        {
            studentState.Pleasure++;
            studentState.DaysCompleted++;
        }

        var dayResult = new DayResult(today);
        history.Add(dayResult);

        var historyRequest = new SaveDayHistoryRequest
        {
            SemesterId = semesterId,
            DayNumber = currentDay,
            Results = today.Select(result =>
                new SubjectAskResultDto
                {
                    Subject = result.Key.ToString(),
                    WasAsked = result.Value
                }).ToList()
        };

        await historyServiceClient.SaveDayAsync(historyRequest, cancellationToken);

        await repository.SaveDayAsync(semesterId, currentDay, studentState, dayResult, random.State, cancellationToken);

        if (isExpelled)
        {
            return false;
        }

        Console.WriteLine($"Удовольствие: {studentState.Pleasure}");

        Console.WriteLine(
            $"Среднее удовольствие в день: " +
            $"{studentState.Pleasure / studentState.DaysCompleted:F2}");

        Console.WriteLine();

        return true;
    }

    private static int ConvertRuleToInt(TeacherRule rule)
    {
        return rule switch
        {
            TeacherRule.Random => 0,
            TeacherRule.IfAskedYesterday => 1,
            TeacherRule.IfAskedHistory => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(rule))
        };
    }

    public StudentState GetStudentState()
    {
        return studentState;
    }
}
