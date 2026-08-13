using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteDatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteDatabaseInitializer(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync(
        AreaId defaultAreaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        await using (var schemaCommand = connection.CreateCommand())
        {
            schemaCommand.Transaction = transaction;

            schemaCommand.CommandText =
                """
                CREATE TABLE IF NOT EXISTS areas
                (
                    id TEXT NOT NULL PRIMARY KEY
                );

                CREATE TABLE IF NOT EXISTS goals
                (
                    id          TEXT NOT NULL PRIMARY KEY,
                    area_id     TEXT NOT NULL,
                    name        TEXT NOT NULL,
                    description TEXT NULL,

                    FOREIGN KEY (area_id)
                        REFERENCES areas (id)
                        ON UPDATE RESTRICT
                        ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ix_goals_area_id
                    ON goals (area_id);

                CREATE INDEX IF NOT EXISTS ix_goals_name
                    ON goals (name);
                """;

            await schemaCommand.ExecuteNonQueryAsync(
                cancellationToken);
        }

        await using (var areaCommand = connection.CreateCommand())
        {
            areaCommand.Transaction = transaction;

            areaCommand.CommandText =
                """
                INSERT OR IGNORE INTO areas (id)
                VALUES ($areaId);
                """;

            areaCommand.Parameters.AddWithValue(
                "$areaId",
                defaultAreaId.Value.ToString());

            await areaCommand.ExecuteNonQueryAsync(
                cancellationToken);
        }

        transaction.Commit();
    }
}
