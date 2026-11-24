using Npgsql;
using Truant.Models;

namespace Truant.Persistance;

public class SemesterRepository
{
    private readonly DatabaseConnectionFactory connectionFactory;

    public SemesterRepository(
        DatabaseConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // Создание нового семестра
    public async Task<long> CreateSemesterAsync(
        StudentState student,
        int currentDay,
        long randomState,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO semesters
            (
                current_day,
                pleasure,
                missed_lessons,
                days_completed,
                is_expelled,
                status,
                random_state
            )
            VALUES
            (
                @currentDay,
                @pleasure,
                @missedLessons,
                @daysCompleted,
                @isExpelled,
                @status,
                @randomState
            )
            RETURNING id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("currentDay", currentDay);
        command.Parameters.AddWithValue("pleasure", student.Pleasure);
        command.Parameters.AddWithValue("missedLessons", student.MissedLessons);
        command.Parameters.AddWithValue("daysCompleted", student.DaysCompleted);
        command.Parameters.AddWithValue("isExpelled", student.IsExpelled);

        string status = student.IsExpelled
            ? "expelled"
            : "in_progress";

        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("randomState", randomState);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return (long)result!;
    }

    // Сохранение преподавателей
    public async Task SaveTeachersAsync(
        long semesterId,
        IEnumerable<Teacher> teachers,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        const string sql = """
            INSERT INTO teachers
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
            );
            """;

        foreach (Teacher teacher in teachers)
        {
            await using var command =
                new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.AddWithValue("semesterId", semesterId);
            command.Parameters.AddWithValue("subject", (int)teacher.Subject);
            command.Parameters.AddWithValue("rule", (int)teacher.Rule);

            command.Parameters.AddWithValue(
                "subjectA",
                teacher.SubjectA.HasValue
                    ? (object)(int)teacher.SubjectA.Value
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "subjectB",
                teacher.SubjectB.HasValue
                    ? (object)(int)teacher.SubjectB.Value
                    : DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    // Сохранение результатов одного дня
    public async Task SaveDayAsync(
        long semesterId,
        int currentDay,
        StudentState student,
        DayResult dayResult,
        long randomState,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        string status = student.IsExpelled
            ? "expelled"
            : "in_progress";

        const string updateSql = """
        UPDATE semesters
        SET
            current_day = @currentDay,
            pleasure = @pleasure,
            missed_lessons = @missedLessons,
            days_completed = @daysCompleted,
            is_expelled = @isExpelled,
            status = @status,
            random_state = @randomState,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = @semesterId;
        """;

        await using (var updateCommand =
            new NpgsqlCommand(updateSql, connection, transaction))
        {
            updateCommand.Parameters.AddWithValue("currentDay", currentDay);
            updateCommand.Parameters.AddWithValue("pleasure", student.Pleasure);
            updateCommand.Parameters.AddWithValue("missedLessons", student.MissedLessons);
            updateCommand.Parameters.AddWithValue("daysCompleted", student.DaysCompleted);
            updateCommand.Parameters.AddWithValue("isExpelled", student.IsExpelled);
            updateCommand.Parameters.AddWithValue("status", status);
            updateCommand.Parameters.AddWithValue("randomState", randomState);
            updateCommand.Parameters.AddWithValue("semesterId", semesterId);

            int affectedRows =
                await updateCommand.ExecuteNonQueryAsync(cancellationToken);

            if (affectedRows != 1)
            {
                throw new InvalidOperationException(
                    $"Семестр с ID {semesterId} не найден.");
            }
        }

        const string insertSql = """
        INSERT INTO day_results
        (
            semester_id,
            day_number,
            subject,
            was_asked
        )
        VALUES
        (
            @semesterId,
            @dayNumber,
            @subject,
            @wasAsked
        );
        """;

        foreach (var entry in dayResult.Asked)
        {
            await using var insertCommand =
                new NpgsqlCommand(insertSql, connection, transaction);

            insertCommand.Parameters.AddWithValue("semesterId", semesterId);
            insertCommand.Parameters.AddWithValue("dayNumber", currentDay);
            insertCommand.Parameters.AddWithValue("subject", (int)entry.Key);
            insertCommand.Parameters.AddWithValue("wasAsked", entry.Value);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<SemesterSnapshot?> GetSemesterAsync(
    long semesterId,
    CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT id, current_day, pleasure, missed_lessons,
               days_completed, is_expelled, status, random_state
        FROM semesters
        WHERE id = @semesterId;
        """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("semesterId", semesterId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var student = new StudentState
        {
            Pleasure = reader.GetDouble(2),
            MissedLessons = reader.GetInt32(3),
            DaysCompleted = reader.GetInt32(4),
            IsExpelled = reader.GetBoolean(5)
        };

        return new SemesterSnapshot
        {
            Id = reader.GetInt64(0),
            CurrentDay = reader.GetInt32(1),
            Student = student,
            Status = reader.GetString(6),
            RandomState = reader.GetInt64(7)
        };
    }

    public async Task<List<Teacher>> GetTeachersAsync(
    long semesterId,
    CancellationToken cancellationToken = default)
    {
        var teachers = new List<Teacher>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT subject, rule, subject_a, subject_b
        FROM teachers
        WHERE semester_id = @semesterId
        ORDER BY subject;
        """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("semesterId", semesterId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            Subject subject = (Subject)reader.GetInt32(0);
            TeacherRule rule = (TeacherRule)reader.GetInt32(1);

            Subject? subjectA = reader.IsDBNull(2)
                ? null
                : (Subject)reader.GetInt32(2);

            Subject? subjectB = reader.IsDBNull(3)
                ? null
                : (Subject)reader.GetInt32(3);

            teachers.Add(new Teacher(subject, rule, subjectA, subjectB));
        }

        return teachers;
    }

    public async Task<List<DayResult>> GetHistoryAsync(
    long semesterId,
    CancellationToken cancellationToken = default)
    {
        var days = new SortedDictionary<int, Dictionary<Subject, bool>>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT day_number, subject, was_asked
        FROM day_results
        WHERE semester_id = @semesterId
        ORDER BY day_number, subject;
        """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("semesterId", semesterId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            int dayNumber = reader.GetInt32(0);
            Subject subject = (Subject)reader.GetInt32(1);
            bool wasAsked = reader.GetBoolean(2);

            if (!days.ContainsKey(dayNumber))
                days[dayNumber] = new Dictionary<Subject, bool>();

            days[dayNumber][subject] = wasAsked;
        }

        return days.Values
            .Select(day => new DayResult(day))
            .ToList();
    }

    public async Task<SemesterSnapshot?> LoadSemesterAsync(
    long semesterId,
    CancellationToken cancellationToken = default)
    {
        SemesterSnapshot? snapshot = await GetSemesterAsync(
            semesterId, cancellationToken);

        if (snapshot == null)
            return null;

        snapshot.Teachers = await GetTeachersAsync(
            semesterId, cancellationToken);

        snapshot.History = await GetHistoryAsync(
            semesterId, cancellationToken);

        return snapshot;
    }

    public async Task<SemesterSnapshot?> GetLatestUnfinishedSemesterAsync(
    CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT id
        FROM semesters
        WHERE status = 'in_progress'
        ORDER BY updated_at DESC, id DESC
        LIMIT 1;
        """;

        await using var command = new NpgsqlCommand(sql, connection);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        if (result == null)
            return null;

        long semesterId = (long)result;

        return await LoadSemesterAsync(semesterId, cancellationToken);
    }
}