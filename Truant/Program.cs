using Truant.Simulation;

Random random = new Random();

var simulator = new SemesterSimulator(random);

var result = simulator.Run();

Console.WriteLine("РЕЗУЛЬТАТ");

if (result.IsExpelled)
{
    Console.WriteLine("Студент вылетел из университета.");
}
else
{
    Console.WriteLine("Студент успешно закончил семестр.");
}

Console.WriteLine($"Пропущено пар: {result.MissedLessons}");
Console.WriteLine($"Общее удовольствие: {result.Pleasure}");

double averagePleasure =
    result.DaysCompleted > 0
        ? result.Pleasure / result.DaysCompleted
        : 0;

Console.WriteLine(
    $"Среднее удовольствие в день: {averagePleasure:F2}");