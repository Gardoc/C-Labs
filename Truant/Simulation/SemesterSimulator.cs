using System;
using Truant.Models;
using Truant.Persistance;
using Truant.Rules;
using Truant.Strategies;

namespace Truant.Simulation
{
    public class SemesterSimulator
    {
        private RandomGenerator random;
        private List<Teacher> teachers;
        private readonly StudentStrategy strategy;
        private StudentState studentState;
        private List<DayResult> history;
        private readonly SemesterRepository repository;
        private int currentDay;
        private long semesterId;

        public SemesterSimulator(RandomGenerator random, StudentStrategy strategy, SemesterRepository repository)
        {
            this.random = random;
            this.strategy = strategy;
            this.repository = repository;

            teachers = new List<Teacher>();

            studentState = new StudentState();
            history = new List<DayResult>();
            currentDay = 0;
        }

        // Создание нового семестра
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
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

            semesterId = await repository.CreateSemesterAsync(studentState, currentDay, random.State, cancellationToken);

            await repository.SaveTeachersAsync(semesterId, teachers, cancellationToken);

            Console.WriteLine($"Создан новый семестр. ID: {semesterId}");
        }

        // Восстановление существующего семестра
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

            // Восстанавливаем состояние генератора случайных чисел
            random = new RandomGenerator(unchecked((ulong)snapshot.RandomState));

            Console.WriteLine("Семестр успешно восстановлен!");
            Console.WriteLine($"ID: {semesterId}");
            Console.WriteLine($"Продолжаем с дня: {currentDay + 1}");
            Console.WriteLine($"Преподавателей: {teachers.Count}");
            Console.WriteLine($"Дней в истории: {history.Count}");
            Console.WriteLine();
        }

        // Выполнение одного дня
        public async Task<bool> RunDayAsync(CancellationToken cancellationToken = default)
        {
            currentDay++;

            Console.WriteLine($"День {currentDay}");

            var decisions = strategy.Decide(history);

            var today = new Dictionary<Subject, bool>();

            bool isExpelled = false;

            foreach (Teacher teacher in teachers)
            {
                bool willAsk = TeacherRuleLogic.ShouldAsk(teacher, GetYesterday(history), () => random.NextDouble());

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

            await repository.SaveDayAsync(semesterId, currentDay, studentState, dayResult, random.State, cancellationToken);

            if (isExpelled)
                return false;

            Console.WriteLine($"Удовольствие: {studentState.Pleasure}");

            Console.WriteLine(
                $"Среднее удовольствие в день: " +
                $"{studentState.Pleasure / studentState.DaysCompleted:F2}");

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