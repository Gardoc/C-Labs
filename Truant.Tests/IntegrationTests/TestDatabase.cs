using Truant.Persistance;

namespace Truant.Tests.IntegrationTests;

public class TestDatabase
{
    public SemesterRepository Repository { get; }

    public TestDatabase()
    {
        string connectionString = "Host=localhost;Port=5432;Database=truant_db_test;Username=postgres;Password=Doctor25";

        var connectionFactory = new DatabaseConnectionFactory(connectionString);

        Repository = new SemesterRepository(connectionFactory);
    }
}