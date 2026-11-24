using Npgsql;
using Truant.Contracts.History;

namespace Truant.History.Service.Data;

public class HistoryRepository
{
    private readonly string connectionString;

    public HistoryRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task SaveDayAsync(SaveDayHistoryRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string sql = """
            INSERT INTO day_results (semester_id, day_number, subject, was_asked)
            VALUES (@semesterId, @dayNumber, @subject, @wasAsked)
            ON CONFLICT (semester_id, day_number, subject)
            DO UPDATE SET was_asked = EXCLUDED.was_asked;
            """;

        try
        {
            foreach (SubjectAskResultDto result in request.Results)
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.AddWithValue("semesterId", request.SemesterId);
                command.Parameters.AddWithValue("dayNumber", request.DayNumber);
                command.Parameters.AddWithValue("subject", result.Subject);
                command.Parameters.AddWithValue("wasAsked", result.WasAsked);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<YesterdayHistoryResponse> GetYesterdayAsync(long semesterId, int dayNumber, string subject, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT was_asked
            FROM day_results
            WHERE semester_id = @semesterId
              AND day_number = @dayNumber
              AND subject = @subject;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("semesterId", semesterId);
        command.Parameters.AddWithValue("dayNumber", dayNumber - 1);
        command.Parameters.AddWithValue("subject", subject);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return new YesterdayHistoryResponse
        {
            Subject = subject,
            WasAsked = result != null && (bool)result
        };
    }
}