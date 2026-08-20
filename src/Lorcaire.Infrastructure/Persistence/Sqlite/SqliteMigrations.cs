using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

internal static class SqliteMigrations
{
    public static IReadOnlyList<SqliteMigration> All { get; } =
    [
        SqliteMigration.FromScript(
            1,
            "Create areas and goals",
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
            """),

        new SqliteMigration(
            2,
            "Add goal completion state",
            requiresBackup: false,
            AddGoalCompletionStateAsync),

        SqliteMigration.FromScript(
            3,
            "Create projects",
            """
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
            """),

        SqliteMigration.FromScript(
            4,
            "Create tasks",
            """
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
            """),

        SqliteMigration.FromScript(
            5,
            "Create resources and calendar events",
            """
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
            """),

        SqliteMigration.FromScript(
            6,
            "Create notes",
            """
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
            """),

        new SqliteMigration(
            7,
            "Normalize calendar event timestamps to UTC",
            requiresBackup: true,
            NormalizeCalendarEventTimestampsAsync)
    ];

    private static async Task AddGoalCompletionStateAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        if (await ColumnExistsAsync(
                connection,
                transaction,
                "goals",
                "is_completed",
                cancellationToken))
        {
            return;
        }

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            ALTER TABLE goals
            ADD COLUMN is_completed INTEGER NOT NULL DEFAULT 0;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> ColumnExistsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"PRAGMA table_info(\"{table}\");";

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(
                    reader.GetString(1),
                    column,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task NormalizeCalendarEventTimestampsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        var events = new List<(string Id, string StartAt, string? EndAt)>();

        await using (var readCommand = connection.CreateCommand())
        {
            readCommand.Transaction = transaction;
            readCommand.CommandText =
                "SELECT id, start_at, end_at FROM calendar_events;";

            await using var reader =
                await readCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                events.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)));
            }
        }

        foreach (var calendarEvent in events)
        {
            await using var updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText =
                """
                UPDATE calendar_events
                SET start_at = $startAt,
                    end_at = $endAt
                WHERE id = $id;
                """;
            updateCommand.Parameters.AddWithValue(
                "$id",
                calendarEvent.Id);
            updateCommand.Parameters.AddWithValue(
                "$startAt",
                NormalizeUtc(calendarEvent.StartAt));
            updateCommand.Parameters.AddWithValue(
                "$endAt",
                calendarEvent.EndAt is null
                    ? DBNull.Value
                    : NormalizeUtc(calendarEvent.EndAt));
            await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string NormalizeUtc(string value) =>
        DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind)
            .ToUniversalTime()
            .ToString("O", CultureInfo.InvariantCulture);
}
