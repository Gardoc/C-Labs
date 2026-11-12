using Npgsql;

namespace Truant.Persistance;

public class DatabaseConnectionFactory
{
    private readonly string connectionString;

    public DatabaseConnectionFactory(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}