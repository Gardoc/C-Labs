using Truant.Teacher.Service.Data;
using Truant.Teacher.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = builder.Configuration.GetConnectionString("TeacherConnection")
    ?? throw new InvalidOperationException("Строка подключения TeacherConnection не найдена.");

string historyBaseUrl = builder.Configuration["HistoryService:BaseUrl"]
    ?? throw new InvalidOperationException("Адрес History Service не найден.");

builder.Services.AddSingleton(new TeacherRepository(connectionString));

builder.Services.AddHttpClient<TeacherLogicService>(client =>
{
    client.BaseAddress = new Uri(historyBaseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();