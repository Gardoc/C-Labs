using Truant.Models;
using Truant.Rules;
using Truant.Strategies;

namespace Truant.Simulation;

internal class SemesterSimulator
{
    private readonly Random random;
    private readonly List<Teacher> teachers;
    private readonly StudentStrategy strategy;

    public SemesterSimulator(Random random)
    {
        this.random = random;

        teachers = TeacherGenerator.CreateTeachers(this.random);
        Console.WriteLine("Преподаватели:");
        foreach (Teacher teacher in teachers)
        {
            Console.WriteLine($"{teacher.Subject}: Правило = {teacher.Rule}, A = {teacher.SubjectA}, B = {teacher.SubjectB}");
        }
        Console.WriteLine();

        strategy = new StudentStrategy();
    }

    public bool RunDay(StudentState student, List<DayResult> history)
    {
        var decisions = strategy.Decide(history, student.Pleasure);

        var today = new Dictionary<Subject, bool>();

        var yesterday = GetYesterday(history);

        foreach (Teacher teacher in teachers)
        {
            bool willAsk = TeacherRuleLogic.ShouldAsk(teacher, yesterday, random);

            today[teacher.Subject] = willAsk;

            bool willAttend = decisions[teacher.Subject];

            Console.WriteLine(
                $"{teacher.Subject}: " +
                $"студент {(willAttend ? "пришел" : "прогулял")}, " +
                $"преподаватель {(willAsk ? "спросил" : "не спросил")}");
            
            if (willAttend)
            {
                continue;
            }
            student.MissedLessons++;
            student.Pleasure++;

            if (willAsk)
            {
                student.IsExpelled = true;
                student.Pleasure = 0;

                Console.WriteLine("СТУДЕНТ ВЫЛЕТЕЛ!");

                return false;
            }            
        }

        student.Pleasure++;
        student.DaysCompleted++;

        history.Add(new DayResult(today));

        Console.WriteLine($"Удовольствие: {student.Pleasure}");

        Console.WriteLine();

        return true;
    }    

    public StudentState Run()
    {
        var student = new StudentState();

        // Вся история наблюдений студента.
        var history = new List<DayResult>();

        for (int day = 1; day <= 100; day++)
        {
            Console.WriteLine($"День {day}");
            bool isRunning = RunDay(student, history);
            if (!isRunning)
            {
                return student;
            }
        }
        return student;
    }

    private static Dictionary<Subject, bool> GetYesterday(
        List<DayResult> history)
    {
        if (history.Count == 0)
        {
            return CreateEmptyHistory();
        }

        return history[^1].Asked;
    }

    private static Dictionary<Subject, bool> CreateEmptyHistory()
    {
        var history = new Dictionary<Subject, bool>();

        foreach (Subject subject in Enum.GetValues<Subject>())
        {
            history[subject] = false;
        }

        return history;
    }
}
