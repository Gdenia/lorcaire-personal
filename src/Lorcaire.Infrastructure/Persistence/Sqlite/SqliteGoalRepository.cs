using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteGoalRepository :
    IGoalRepository,
    IGoalReader
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteGoalRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(goal);

        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO goals
            (
                id,
                area_id,
                name,
                description,
                is_completed
            )
            VALUES
            (
                $id,
                $areaId,
                $name,
                $description,
                $isCompleted
            );
            """;

        command.Parameters.AddWithValue(
            "$id",
            goal.Id.Value.ToString());

        command.Parameters.AddWithValue(
            "$areaId",
            goal.AreaId.Value.ToString());

        command.Parameters.AddWithValue(
            "$name",
            goal.Name);

        command.Parameters.AddWithValue(
            "$description",
            goal.Description is null
                ? DBNull.Value
                : goal.Description);

        command.Parameters.AddWithValue(
            "$isCompleted",
            goal.IsCompleted ? 1 : 0);

        try
        {
            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }
        catch (SqliteException exception)
            when (exception.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException(
                "No se pudo guardar el objetivo porque sus datos " +
                "incumplen una restricción de integridad.",
                exception);
        }
    }

    public async Task<Goal?> GetByIdAsync(
        GoalId goalId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, area_id, name, description, is_completed
            FROM goals
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", goalId.Value.ToString());

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? ReadGoal(reader)
            : null;
    }

    public async Task UpdateAsync(
        Goal goal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(goal);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE goals
            SET area_id = $areaId,
                name = $name,
                description = $description,
                is_completed = $isCompleted
            WHERE id = $id;
            """;
        AddParameters(command, goal);

        try
        {
            var affectedRows =
                await command.ExecuteNonQueryAsync(cancellationToken);

            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    $"No existe un objetivo con identificador '{goal.Id}'.");
            }
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw CreateIntegrityException(exception);
        }
    }

    public async Task<bool> DeleteAsync(
        GoalId goalId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM goals WHERE id = $id;";
        command.Parameters.AddWithValue("$id", goalId.Value.ToString());

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException(
                "The goal cannot be deleted because other information depends on it.",
                exception);
        }
    }

    public async Task<IReadOnlyList<Goal>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT
                id,
                area_id,
                name,
                description,
                is_completed
            FROM goals
            ORDER BY name COLLATE NOCASE;
            """;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var goals = new List<Goal>();

        while (await reader.ReadAsync(cancellationToken))
        {
            goals.Add(ReadGoal(reader));
        }

        return goals;
    }

    private static Goal ReadGoal(SqliteDataReader reader) =>
        new(
            new GoalId(Guid.Parse(reader.GetString(0))),
            new AreaId(Guid.Parse(reader.GetString(1))),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetInt64(4) == 1);

    private static void AddParameters(SqliteCommand command, Goal goal)
    {
        command.Parameters.AddWithValue("$id", goal.Id.Value.ToString());
        command.Parameters.AddWithValue("$areaId", goal.AreaId.Value.ToString());
        command.Parameters.AddWithValue("$name", goal.Name);
        command.Parameters.AddWithValue(
            "$description",
            goal.Description is null ? DBNull.Value : goal.Description);
        command.Parameters.AddWithValue("$isCompleted", goal.IsCompleted ? 1 : 0);
    }

    private static InvalidOperationException CreateIntegrityException(
        SqliteException exception) =>
        new(
            "No se pudo guardar el objetivo porque sus datos " +
            "incumplen una restricción de integridad.",
            exception);
}
