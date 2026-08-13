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
                description
            )
            VALUES
            (
                $id,
                $areaId,
                $name,
                $description
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
                description
            FROM goals
            ORDER BY name COLLATE NOCASE;
            """;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var goals = new List<Goal>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var goalId = new GoalId(
                Guid.Parse(reader.GetString(0)));

            var areaId = new AreaId(
                Guid.Parse(reader.GetString(1)));

            var name = reader.GetString(2);

            var description = reader.IsDBNull(3)
                ? null
                : reader.GetString(3);

            goals.Add(
                new Goal(
                    goalId,
                    areaId,
                    name,
                    description));
        }

        return goals;
    }
}
