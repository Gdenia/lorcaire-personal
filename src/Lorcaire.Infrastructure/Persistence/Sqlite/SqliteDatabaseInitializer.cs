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
                    id           TEXT NOT NULL PRIMARY KEY,
                    area_id      TEXT NOT NULL,
                    name         TEXT NOT NULL,
                    description  TEXT NULL,
                    is_completed INTEGER NOT NULL DEFAULT 0,

                    FOREIGN KEY (area_id)
                        REFERENCES areas (id)
                        ON UPDATE RESTRICT
                        ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ix_goals_area_id
                    ON goals (area_id);

                CREATE INDEX IF NOT EXISTS ix_goals_name
                    ON goals (name);

                CREATE TABLE IF NOT EXISTS projects
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

                CREATE INDEX IF NOT EXISTS ix_projects_area_id
                    ON projects (area_id);

                CREATE INDEX IF NOT EXISTS ix_projects_name
                    ON projects (name);

                CREATE TABLE IF NOT EXISTS tasks
                (
                    id           TEXT NOT NULL PRIMARY KEY,
                    area_id      TEXT NOT NULL,
                    title        TEXT NOT NULL,
                    description  TEXT NULL,
                    is_completed INTEGER NOT NULL DEFAULT 0,

                    FOREIGN KEY (area_id)
                        REFERENCES areas (id)
                        ON UPDATE RESTRICT
                        ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ix_tasks_area_id
                    ON tasks (area_id);

                CREATE INDEX IF NOT EXISTS ix_tasks_title
                    ON tasks (title);

                CREATE TABLE IF NOT EXISTS resources
                (
                    id          TEXT NOT NULL PRIMARY KEY,
                    area_id     TEXT NOT NULL,
                    name        TEXT NOT NULL,
                    category    TEXT NOT NULL,
                    description TEXT NULL,

                    FOREIGN KEY (area_id)
                        REFERENCES areas (id)
                        ON UPDATE RESTRICT
                        ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ix_resources_area_id
                    ON resources (area_id);

                CREATE INDEX IF NOT EXISTS ix_resources_category_name
                    ON resources (category, name);

                CREATE TABLE IF NOT EXISTS calendar_events
                (
                    id          TEXT NOT NULL PRIMARY KEY,
                    area_id     TEXT NOT NULL,
                    title       TEXT NOT NULL,
                    description TEXT NULL,
                    start_at    TEXT NOT NULL,
                    end_at      TEXT NULL,

                    FOREIGN KEY (area_id)
                        REFERENCES areas (id)
                        ON UPDATE RESTRICT
                        ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ix_calendar_events_area_id
                    ON calendar_events (area_id);

                CREATE INDEX IF NOT EXISTS ix_calendar_events_start_at
                    ON calendar_events (start_at);

                CREATE TABLE IF NOT EXISTS notes
                (
                    id               TEXT NOT NULL PRIMARY KEY,
                    area_id          TEXT NOT NULL,
                    title            TEXT NOT NULL,
                    content          TEXT NOT NULL,
                    created_at       TEXT NOT NULL,
                    last_modified_at TEXT NOT NULL,

                    FOREIGN KEY (area_id)
                        REFERENCES areas (id)
                        ON UPDATE RESTRICT
                        ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ix_notes_area_id
                    ON notes (area_id);

                CREATE INDEX IF NOT EXISTS ix_notes_last_modified_at
                    ON notes (last_modified_at DESC);
                """;

            await schemaCommand.ExecuteNonQueryAsync(
                cancellationToken);
        }

        var hasIsCompletedColumn = false;

        await using (var checkCommand = connection.CreateCommand())
        {
            checkCommand.Transaction = transaction;
            checkCommand.CommandText = "PRAGMA table_info(goals);";

            await using var reader =
                await checkCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.GetString(1) == "is_completed")
                {
                    hasIsCompletedColumn = true;
                    break;
                }
            }
        }

        if (!hasIsCompletedColumn)
        {
            await using var migrationCommand =
                connection.CreateCommand();

            migrationCommand.Transaction = transaction;

            migrationCommand.CommandText =
                """
                ALTER TABLE goals
                ADD COLUMN is_completed INTEGER NOT NULL DEFAULT 0;
                """;

            await migrationCommand.ExecuteNonQueryAsync(
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
