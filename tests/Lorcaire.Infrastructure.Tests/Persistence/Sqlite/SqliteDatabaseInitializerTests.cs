using Lorcaire.Core.Domain.Areas;
using Lorcaire.Infrastructure.Persistence.Sqlite;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteDatabaseInitializerTests
{
    [Fact]
    public async Task InitializeAsync_CreatesCurrentSchemaFromScratch()
    {
        await using var database = TemporaryDatabase.Create();
        var areaId = AreaId.New();
        var initializer = new SqliteDatabaseInitializer(database.Factory);

        await initializer.InitializeAsync(areaId);

        Assert.Equal(8, initializer.CurrentSchemaVersion);
        Assert.Equal(
            [1, 2, 3, 4, 5, 6, 7, 8],
            await ReadVersionsAsync(database.Factory));
        Assert.Equal(
            1L,
            await ExecuteScalarAsync<long>(
                database.Factory,
                "SELECT COUNT(*) FROM areas WHERE id = $id;",
                ("$id", areaId.Value.ToString())));
        Assert.Empty(Directory.GetFiles(database.DirectoryPath, "*.backup-*"));
    }

    [Fact]
    public async Task InitializeAsync_UpgradesLegacySchemaAndPreservesData()
    {
        await using var database = TemporaryDatabase.Create();
        var areaId = AreaId.New();
        var goalId = Guid.NewGuid();

        await ExecuteAsync(
            database.Factory,
            """
            CREATE TABLE areas
            (
                id TEXT NOT NULL PRIMARY KEY
            );

            CREATE TABLE goals
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

            CREATE INDEX ix_goals_area_id ON goals (area_id);
            CREATE INDEX ix_goals_name ON goals (name);

            INSERT INTO areas (id) VALUES ($areaId);
            INSERT INTO goals (id, area_id, name, description)
            VALUES ($goalId, $areaId, 'Legacy goal', 'Preserved');
            """,
            ("$areaId", areaId.Value.ToString()),
            ("$goalId", goalId.ToString()));

        await new SqliteDatabaseInitializer(database.Factory)
            .InitializeAsync(areaId);

        Assert.Equal(
            [1, 2, 3, 4, 5, 6, 7, 8],
            await ReadVersionsAsync(database.Factory));
        Assert.Equal(
            "Legacy goal|Preserved|0",
            await ExecuteScalarAsync<string>(
                database.Factory,
                """
                SELECT name || '|' || description || '|' || is_completed
                FROM goals
                WHERE id = $id;
                """,
                ("$id", goalId.ToString())));
        Assert.Equal(
            1L,
            await ExecuteScalarAsync<long>(
                database.Factory,
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table' AND name = 'notes';
                """));
    }

    [Fact]
    public async Task InitializeAsync_RecordsMigrationsInHistoricalOrder()
    {
        await using var database = TemporaryDatabase.Create();

        await new SqliteDatabaseInitializer(database.Factory)
            .InitializeAsync(AreaId.New());

        var names = await ReadMigrationNamesAsync(database.Factory);

        Assert.Equal(
            [
                "Create areas and goals",
                "Add goal completion state",
                "Create projects",
                "Create tasks",
                "Create resources and calendar events",
                "Create notes",
                "Normalize calendar event timestamps to UTC",
                "Assign tasks to optional projects"
            ],
            names);
    }

    [Fact]
    public async Task InitializeAsync_NormalizesExistingCalendarDataWithoutChangingInstants()
    {
        await using var database = TemporaryDatabase.Create();
        var areaId = AreaId.New();
        var eventId = Guid.NewGuid();
        var versionSixInitializer = new SqliteDatabaseInitializer(
            database.Factory,
            SqliteMigrations.All.Take(6).ToArray());
        await versionSixInitializer.InitializeAsync(areaId);
        await ExecuteAsync(
            database.Factory,
            """
            INSERT INTO calendar_events
                (id, area_id, title, start_at, end_at)
            VALUES
                ($id, $areaId, 'DST event', $startAt, $endAt);
            """,
            ("$id", eventId.ToString()),
            ("$areaId", areaId.Value.ToString()),
            ("$startAt", "2026-10-25T02:30:00.0000000+02:00"),
            ("$endAt", "2026-10-25T02:30:00.0000000+01:00"));

        await new SqliteDatabaseInitializer(database.Factory)
            .InitializeAsync(areaId);

        Assert.Equal(
            "2026-10-25T00:30:00.0000000+00:00|" +
            "2026-10-25T01:30:00.0000000+00:00",
            await ExecuteScalarAsync<string>(
                database.Factory,
                """
                SELECT start_at || '|' || end_at
                FROM calendar_events
                WHERE id = $id;
                """,
                ("$id", eventId.ToString())));
        Assert.Equal(
            [1, 2, 3, 4, 5, 6, 7, 8],
            await ReadVersionsAsync(database.Factory));

        var backupPath = Assert.Single(
            Directory.GetFiles(
                database.DirectoryPath,
                "lorcaire.db.backup-v7-*"));
        Assert.Equal(
            "2026-10-25T02:30:00.0000000+02:00",
            await ExecuteScalarAsync<string>(
                new SqliteConnectionFactory(backupPath),
                "SELECT start_at FROM calendar_events WHERE id = $id;",
                ("$id", eventId.ToString())));
    }

    [Fact]
    public async Task InitializeAsync_AddsNullableProjectToExistingTasks()
    {
        await using var database = TemporaryDatabase.Create();
        var areaId = AreaId.New();
        var taskId = Guid.NewGuid();
        await new SqliteDatabaseInitializer(
                database.Factory,
                SqliteMigrations.All.Take(7).ToArray())
            .InitializeAsync(areaId);
        await ExecuteAsync(
            database.Factory,
            """
            INSERT INTO tasks
                (id, area_id, title, description, is_completed)
            VALUES
                ($id, $areaId, 'Existing task', NULL, 1);
            """,
            ("$id", taskId.ToString()),
            ("$areaId", areaId.Value.ToString()));

        await new SqliteDatabaseInitializer(database.Factory)
            .InitializeAsync(areaId);

        Assert.Equal(
            "Existing task|1|none",
            await ExecuteScalarAsync<string>(
                database.Factory,
                """
                SELECT title || '|' || is_completed || '|' ||
                       COALESCE(project_id, 'none')
                FROM tasks
                WHERE id = $id;
                """,
                ("$id", taskId.ToString())));
        Assert.Equal(
            [1, 2, 3, 4, 5, 6, 7, 8],
            await ReadVersionsAsync(database.Factory));
    }

    [Fact]
    public async Task InitializeAsync_CompletesRecognizedPartialLegacySchema()
    {
        await using var database = TemporaryDatabase.Create();
        var areaId = AreaId.New();
        var projectId = Guid.NewGuid();
        await ExecuteAsync(
            database.Factory,
            """
            CREATE TABLE areas (id TEXT NOT NULL PRIMARY KEY);
            CREATE TABLE goals
            (
                id TEXT NOT NULL PRIMARY KEY,
                area_id TEXT NOT NULL,
                name TEXT NOT NULL,
                description TEXT NULL,
                is_completed INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (area_id) REFERENCES areas (id)
                    ON UPDATE RESTRICT ON DELETE RESTRICT
            );
            CREATE TABLE projects
            (
                id TEXT NOT NULL PRIMARY KEY,
                area_id TEXT NOT NULL,
                name TEXT NOT NULL,
                description TEXT NULL,
                FOREIGN KEY (area_id) REFERENCES areas (id)
                    ON UPDATE RESTRICT ON DELETE RESTRICT
            );
            INSERT INTO areas (id) VALUES ($areaId);
            INSERT INTO projects (id, area_id, name)
            VALUES ($projectId, $areaId, 'Partial project');
            """,
            ("$areaId", areaId.Value.ToString()),
            ("$projectId", projectId.ToString()));

        await new SqliteDatabaseInitializer(database.Factory)
            .InitializeAsync(areaId);

        Assert.Equal(
            "Partial project",
            await ExecuteScalarAsync<string>(
                database.Factory,
                "SELECT name FROM projects WHERE id = $id;",
                ("$id", projectId.ToString())));
        Assert.Equal(
            [1, 2, 3, 4, 5, 6, 7, 8],
            await ReadVersionsAsync(database.Factory));
        Assert.Equal(
            1L,
            await ExecuteScalarAsync<long>(
                database.Factory,
                """
                SELECT COUNT(*) FROM sqlite_master
                WHERE type = 'table' AND name = 'calendar_events';
                """));
    }

    [Fact]
    public async Task InitializeAsync_DoesNotRepeatAppliedMigrations()
    {
        await using var database = TemporaryDatabase.Create();
        var applyCount = 0;
        var migrations = SqliteMigrations.All
            .Append(
                new SqliteMigration(
                    9,
                    "Count execution",
                    requiresBackup: false,
                    async (connection, transaction, cancellationToken) =>
                    {
                        applyCount++;
                        await using var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText =
                            "CREATE TABLE migration_execution_probe (id INTEGER);";
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }))
            .ToArray();
        var initializer = new SqliteDatabaseInitializer(
            database.Factory,
            migrations);

        await initializer.InitializeAsync(AreaId.New());
        await initializer.InitializeAsync(AreaId.New());

        Assert.Equal(1, applyCount);
        Assert.Equal(9, (await ReadVersionsAsync(database.Factory)).Count);
    }

    [Fact]
    public async Task InitializeAsync_RollsBackFailedMigrationAndHistoryEntry()
    {
        await using var database = TemporaryDatabase.Create();
        var migrations = SqliteMigrations.All
            .Append(
                new SqliteMigration(
                    9,
                    "Fail after schema change",
                    requiresBackup: false,
                    ApplyFailingMigrationAsync))
            .ToArray();
        var initializer = new SqliteDatabaseInitializer(
            database.Factory,
            migrations);

        var exception = await Assert.ThrowsAsync<SqliteMigrationException>(
            () => initializer.InitializeAsync(AreaId.New()));

        Assert.Equal(9, exception.Version);
        Assert.DoesNotContain(9, await ReadVersionsAsync(database.Factory));
        Assert.Equal(
            0L,
            await ExecuteScalarAsync<long>(
                database.Factory,
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table' AND name = 'rollback_probe';
                """));
    }

    [Fact]
    public async Task InitializeAsync_RejectsNewerSchemaVersion()
    {
        await using var database = TemporaryDatabase.Create();
        var initializer = new SqliteDatabaseInitializer(database.Factory);
        await initializer.InitializeAsync(AreaId.New());
        await ExecuteAsync(
            database.Factory,
            """
            INSERT INTO schema_migrations (version, name, applied_at)
            VALUES (999, 'Future migration', '2026-01-01T00:00:00Z');
            """);

        var exception =
            await Assert.ThrowsAsync<SqliteSchemaVersionTooNewException>(
                () => initializer.InitializeAsync(AreaId.New()));

        Assert.Equal(999, exception.DatabaseVersion);
        Assert.Equal(8, exception.SupportedVersion);
    }

    [Fact]
    public async Task InitializeAsync_CreatesBackupOnlyForRiskyMigration()
    {
        await using var database = TemporaryDatabase.Create();
        await new SqliteDatabaseInitializer(database.Factory)
            .InitializeAsync(AreaId.New());
        var migrations = SqliteMigrations.All
            .Append(
                SqliteMigration.FromScript(
                    9,
                    "Risky migration",
                    "CREATE TABLE backup_probe (id INTEGER);",
                    requiresBackup: true))
            .ToArray();

        await new SqliteDatabaseInitializer(database.Factory, migrations)
            .InitializeAsync(AreaId.New());

        var backupPath = Assert.Single(
            Directory.GetFiles(
                database.DirectoryPath,
                "lorcaire.db.backup-v9-*"));
        var backupFactory = new SqliteConnectionFactory(backupPath);
        Assert.Equal(8L, await ExecuteScalarAsync<long>(
            backupFactory,
            "SELECT MAX(version) FROM schema_migrations;"));
    }

    [Fact]
    public async Task InitializeAsync_RejectsPotentiallyCorruptDatabase()
    {
        await using var database = TemporaryDatabase.Create();
        await File.WriteAllBytesAsync(
            database.DatabasePath,
            "This is not a SQLite database"u8.ToArray());

        await Assert.ThrowsAsync<SqliteDatabaseCorruptException>(
            () => new SqliteDatabaseInitializer(database.Factory)
                .InitializeAsync(AreaId.New()));
    }

    [Fact]
    public async Task InitializeAsync_RejectsUnknownLegacyShapeBeforeMigrating()
    {
        await using var database = TemporaryDatabase.Create();
        await ExecuteAsync(
            database.Factory,
            "CREATE TABLE goals (id TEXT NOT NULL PRIMARY KEY);");

        var exception = await Assert.ThrowsAsync<SqliteMigrationException>(
            () => new SqliteDatabaseInitializer(database.Factory)
                .InitializeAsync(AreaId.New()));

        Assert.Equal("Recognize legacy database", exception.MigrationName);
        Assert.Equal(
            0L,
            await ExecuteScalarAsync<long>(
                database.Factory,
                """
                SELECT COUNT(*) FROM sqlite_master
                WHERE type = 'table' AND name = 'schema_migrations';
                """));
        Assert.Equal(
            1L,
            await ExecuteScalarAsync<long>(
                database.Factory,
                "SELECT COUNT(*) FROM pragma_table_info('goals');"));
    }

    [Fact]
    public void ClassifySqliteException_DistinguishesLockedDatabase()
    {
        var result = SqliteDatabaseInitializer.ClassifySqliteException(
            new SqliteException("database is locked", 5));

        Assert.IsType<SqliteDatabaseLockedException>(result);
    }

    private static async Task ApplyFailingMigrationAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using (var createCommand = connection.CreateCommand())
        {
            createCommand.Transaction = transaction;
            createCommand.CommandText =
                "CREATE TABLE rollback_probe (id INTEGER);";
            await createCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var failingCommand = connection.CreateCommand();
        failingCommand.Transaction = transaction;
        failingCommand.CommandText = "INSERT INTO missing_table VALUES (1);";
        await failingCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<int>> ReadVersionsAsync(
        SqliteConnectionFactory factory)
    {
        var versions = new List<int>();
        await using var connection = factory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT version FROM schema_migrations ORDER BY version;";
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            versions.Add(reader.GetInt32(0));
        }

        return versions;
    }

    private static async Task<IReadOnlyList<string>> ReadMigrationNamesAsync(
        SqliteConnectionFactory factory)
    {
        var names = new List<string>();
        await using var connection = factory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT name FROM schema_migrations ORDER BY version;";
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private static async Task ExecuteAsync(
        SqliteConnectionFactory factory,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        await using var connection = factory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ExecuteScalarAsync<T>(
        SqliteConnectionFactory factory,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        await using var connection = factory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        return (T)(await command.ExecuteScalarAsync())!;
    }

    private sealed class TemporaryDatabase : IAsyncDisposable
    {
        private TemporaryDatabase(string directoryPath)
        {
            DirectoryPath = directoryPath;
            DatabasePath = Path.Combine(directoryPath, "lorcaire.db");
            Factory = new SqliteConnectionFactory(DatabasePath);
        }

        public string DirectoryPath { get; }

        public string DatabasePath { get; }

        public SqliteConnectionFactory Factory { get; }

        public static TemporaryDatabase Create()
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Lorcaire.Tests",
                Guid.NewGuid().ToString("N"));
            return new TemporaryDatabase(directoryPath);
        }

        public ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }

            return ValueTask.CompletedTask;
        }
    }
}
