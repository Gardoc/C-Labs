using System.Reflection.Metadata.Ecma335;
using Truant.Models;
using Truant.Rules;
using Truant.Strategies;

namespace Truant.Simulation
{
    internal class SemesterSimulator
    {
        private readonly Random random;
        private readonly List<Teacher> teachers;
        private readonly StudentStrategy strategy;
        private readonly StudentState studentState;
        private readonly List<DayResult> history;
        private int currentDay;

        public SemesterSimulator(Random random, StudentStrategy strategy)
        {
            this.random = random;
            this.strategy = strategy;

            teachers = TeacherGenerator.CreateTeachers(this.random);
            Console.WriteLine("Преподаватели:");
            foreach (Teacher teacher in teachers)
            {
                Console.WriteLine($"{teacher.Subject}: Правило = {teacher.Rule}, A = {teacher.SubjectA}, B = {teacher.SubjectB}");
            }
            Console.WriteLine();

            studentState = new StudentState();
            history = new List<DayResult>();
            currentDay = 0;
        }


        public bool RunDay()
        {
            currentDay++;

            Console.WriteLine($"День {currentDay}");

            var decisions = strategy.Decide(history, studentState.Pleasure);

            var today = new Dictionary<Subject, bool>();


            foreach (Teacher teacher in teachers)
            {
                bool willAsk = TeacherRuleLogic.ShouldAsk(teacher, GetYesterday(history), random);

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
                studentState.MissedLessons++;
                studentState.Pleasure++;


                if (willAsk)
                {
                    studentState.IsExpelled = true;
                    studentState.Pleasure = 0;

                    Console.WriteLine("СТУДЕНТ ВЫЛЕТЕЛ!");

                    return false;
                }
            }

            studentState.Pleasure++;
            studentState.DaysCompleted++;

            history.Add(new DayResult(today));

            Console.WriteLine($"Удовольствие: {studentState.Pleasure}");
            Console.WriteLine($"Среднее удовольствие в день: {studentState.Pleasure / studentState.DaysCompleted:f2}");
            Console.WriteLine();

            return true;

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

        public StudentState GetStudentState()
        {
            return studentState;
        }
    }
}