using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Truant.Models;
using Truant.Persistance;
using Truant.Services;
using Truant.Simulation;
using Truant.Strategies;
using Truant.Clients;

var startupOptions = new SimulationStartupOptions();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddSingleton<RandomGenerator>();
builder.Services.AddSingleton<StudentStrategy>();
builder.Services.AddSingleton<SemesterSimulator>();
builder.Services.AddSingleton<SemesterRepository>();
string teacherBaseUrl = builder.Configuration["TeacherService:BaseUrl"]
?? throw new InvalidOperationException("Адрес Teacher Service не найден.");

builder.Services.AddHttpClient<TeacherServiceClient>(client =>
{
    client.BaseAddress = new Uri(teacherBaseUrl.TrimEnd('/') + "/");
});
string historyBaseUrl = builder.Configuration["HistoryService:BaseUrl"]
    ?? throw new InvalidOperationException("Адрес History Service не найден.");

builder.Services.AddHttpClient<HistoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(historyBaseUrl.TrimEnd('/') + "/");
});
builder.Services.AddSingleton(startupOptions);

builder.Services.AddSingleton(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Строка подключения не найдена.");

    return new DatabaseConnectionFactory(connectionString);
});

builder.Services.AddHostedService<SemesterHostedService>();
builder.Services.AddHostedService<KeyboardShutdownService>();

var host = builder.Build();

SemesterSnapshot? latestSemester;

using (var scope = host.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<SemesterRepository>();

    latestSemester = await repository.GetLatestUnfinishedSemesterAsync();
}

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

await host.RunAsync();