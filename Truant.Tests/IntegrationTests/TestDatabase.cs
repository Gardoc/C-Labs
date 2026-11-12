using Microsoft.Extensions.Configuration;
using Truant.Persistance;

namespace Truant.Tests.IntegrationTests;

public class TestDatabase
{
    public SemesterRepository Repository { get; }
    public string ConnectionString { get; }

    public TestDatabase()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        ConnectionString = configuration.GetConnectionString("TestConnection")
            ?? throw new InvalidOperationException("Строка подключения TestConnection не найдена.");

        var connectionFactory = new DatabaseConnectionFactory(ConnectionString);
        Repository = new SemesterRepository(connectionFactory);
    }
}