using Npgsql;
using Truant.Teacher.Service.Models;

namespace Truant.Teacher.Service.Data;

public class TeacherRepository
{
    private readonly string connectionString;

    public TeacherRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task SaveConfigurationsAsync(long semesterId, List<TeacherConfiguration> configurations, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string sql = """
            INSERT INTO teacher_configurations
            (
                semester_id,
                subject,
                rule,
                subject_a,
                subject_b
            )
            VALUES
            (
                @semesterId,
                @subject,
                @rule,
                @subjectA,
                @subjectB
            )
            ON CONFLICT (semester_id, subject)
            DO UPDATE SET
                rule = EXCLUDED.rule,
                subject_a = EXCLUDED.subject_a,
                subject_b = EXCLUDED.subject_b;
            """;

        try
        {
            foreach (TeacherConfiguration teacher in configurations)
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.AddWithValue("semesterId", semesterId);
                command.Parameters.AddWithValue("subject", teacher.Subject);
                command.Parameters.AddWithValue("rule", teacher.Rule);
                command.Parameters.AddWithValue("subjectA", (object?)teacher.SubjectA ?? DBNull.Value);
                command.Parameters.AddWithValue("subjectB", (object?)teacher.SubjectB ?? DBNull.Value);

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

    public async Task<TeacherConfiguration?> GetConfigurationAsync(long semesterId, string subject, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT semester_id, subject, rule, subject_a, subject_b
            FROM teacher_configurations
            WHERE semester_id = @semesterId AND subject = @subject;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("semesterId", semesterId);
        command.Parameters.AddWithValue("subject", subject);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new TeacherConfiguration
        {
            SemesterId = reader.GetInt64(0),
            Subject = reader.GetString(1),
            Rule = reader.GetInt32(2),
            SubjectA = reader.IsDBNull(3) ? null : reader.GetString(3),
            SubjectB = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }
}