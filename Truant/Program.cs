using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Truant.Models;
using Truant.Persistance;
using Truant.Services;
using Truant.Simulation;
using Truant.Strategies;

var startupOptions = new SimulationStartupOptions();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Регистрируем зависимости
builder.Services.AddSingleton<RandomGenerator>();
builder.Services.AddSingleton<StudentStrategy>();
builder.Services.AddSingleton<SemesterSimulator>();
builder.Services.AddSingleton<SemesterRepository>();
builder.Services.AddSingleton(startupOptions);

builder.Services.AddSingleton(provider =>
{
    string connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Строка подключения не найдена.");

    return new DatabaseConnectionFactory(connectionString);
});

// Регистрируем фоновые сервисы
builder.Services.AddHostedService<SemesterHostedService>();
builder.Services.AddHostedService<KeyboardShutdownService>();

var host = builder.Build();

// Получаем последний незавершённый семестр
SemesterSnapshot? latestSemester;

using (var scope = host.Services.CreateScope())
{
    var repository = scope.ServiceProvider
        .GetRequiredService<SemesterRepository>();

    latestSemester = await repository.GetLatestUnfinishedSemesterAsync();
}

// Меню запуска
Console.WriteLine("=================================");
Console.WriteLine("       СИМУЛЯТОР СЕМЕСТРА");
Console.WriteLine("=================================");
Console.WriteLine();

Console.WriteLine("1. Начать новый семестр");

if (latestSemester != null)
{
    Console.WriteLine(
        $"2. Продолжить последний незавершённый семестр " +
        $"(ID: {latestSemester.Id}, день: {latestSemester.CurrentDay})");
}
else
{
    Console.WriteLine("2. Продолжить последний семестр (нет доступных)");
}

Console.WriteLine();

while (true)
{
    Console.Write("Выберите режим: ");
    string? choice = Console.ReadLine();

    if (choice == "1")
    {
        startupOptions.ResumeExisting = false;
        break;
    }

    if (choice == "2" && latestSemester != null)
    {
        startupOptions.ResumeExisting = true;
        startupOptions.SemesterId = latestSemester.Id;
        break;
    }

    Console.WriteLine("Некорректный выбор. Попробуйте ещё раз.");
    Console.WriteLine();
}

Console.WriteLine();

// Запускаем приложение
await host.RunAsync();