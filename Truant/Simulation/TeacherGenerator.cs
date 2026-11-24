using Truant.Models;

namespace Truant.Simulation;

public static class TeacherGenerator
{
    public static List<Teacher> CreateTeachers(RandomGenerator random)
    {
        var subjects = Enum.GetValues<Subject>();

        var teachers = new List<Teacher>();

        foreach (var subject in subjects)
        {
            TeacherRule rule = (TeacherRule)random.Next(0, 3);

            switch (rule)
            {
                case TeacherRule.Random:
                    teachers.Add(new Teacher(subject, rule));

                    break;

                case TeacherRule.IfAskedYesterday:
                    {
                        Subject a = subjects[random.Next(0, subjects.Length)];

                        teachers.Add(new Teacher(subject, rule, a));

                        break;
                    }

                case TeacherRule.IfAskedHistory:
                    {
                        Subject a = subjects[random.Next(0, subjects.Length)];

                        Subject b;

                        do
                        {
                            b = subjects[random.Next(0, subjects.Length)];
                        }
                        while (b == a);

                        teachers.Add(new Teacher(subject, rule, a, b));

                        break;
                    }
            }
        }

        return teachers;
    }
}
