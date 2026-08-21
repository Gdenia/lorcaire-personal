using System.Globalization;
using Lorcaire.Core.Domain.Areas;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteDatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly IReadOnlyList<SqliteMigration> _migrations;

    public SqliteDatabaseInitializer(
        SqliteConnectionFactory connectionFactory)
        : this(connectionFactory, SqliteMigrations.All)
    {
    }

    internal SqliteDatabaseInitializer(
        SqliteConnectionFactory connectionFactory,
        IReadOnlyList<SqliteMigration> migrations)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(migrations);

        ValidateMigrationSequence(migrations);

        _connectionFactory = connectionFactory;
        _migrations = migrations;
    }

    public int CurrentSchemaVersion =>
        _migrations.Count == 0 ? 0 : _migrations[^1].Version;

    public async Task InitializeAsync(
        AreaId defaultAreaId,
        CancellationToken cancellationToken = default)
    {
        var databaseExisted =
            File.Exists(_connectionFactory.DatabasePath) &&
            new FileInfo(_connectionFactory.DatabasePath).Length > 0;

        try
        {
            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);
            await EnsureDatabaseIsHealthyAsync(
                connection,
                cancellationToken);

            if (!await TableExistsAsync(
                    connection,
                    "schema_migrations",
                    cancellationToken))
            {
                await ValidateLegacySchemaAsync(
                    connection,
                    cancellationToken);
            }

            await EnsureMigrationTableAsync(
                connection,
                cancellationToken);

            var appliedVersions = await GetAppliedVersionsAsync(
                connection,
                cancellationToken);

            ValidateAppliedVersions(appliedVersions);

            foreach (var migration in _migrations)
            {
                if (appliedVersions.Contains(migration.Version))
                {
                    continue;
                }

                if (migration.RequiresBackup && databaseExisted)
                {
                    CreateBackup(connection, migration.Version);
                }

                await ApplyMigrationAsync(
                    connection,
                    migration,
                    cancellationToken);
            }

            await ValidateSchemaAsync(connection, cancellationToken);

            await EnsureDefaultAreaAsync(
                connection,
                defaultAreaId,
                cancellationToken);
        }
        catch (SqliteDatabaseInitializationException)
        {
            throw;
        }
        catch (SqliteException exception)
        {
            throw ClassifySqliteException(exception);
        }
    }

    private static void ValidateMigrationSequence(
        IReadOnlyList<SqliteMigration> migrations)
    {
        for (var index = 0; index < migrations.Count; index++)
        {
            var expectedVersion = index + 1;

            if (migrations[index].Version != expectedVersion)
            {
                throw new ArgumentException(
                    "Las migraciones SQLite deben comenzar en 1 y ser consecutivas.",
                    nameof(migrations));
            }
        }
    }

    private static async Task EnsureDatabaseIsHealthyAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";

        var result = Convert.ToString(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new SqliteDatabaseCorruptException(
                $"SQLite detectó una base de datos potencialmente corrupta: {result}");
        }
    }

    private static async Task EnsureMigrationTableAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS schema_migrations
            (
                version    INTEGER NOT NULL PRIMARY KEY,
                name       TEXT NOT NULL,
                applied_at TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ValidateLegacySchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var expectedColumns = new Dictionary<string, string[]>
        {
            ["areas"] = ["id"],
            ["projects"] = ["id", "area_id", "name", "description"],
            ["tasks"] =
                ["id", "area_id", "title", "description", "is_completed"],
            ["resources"] =
                ["id", "area_id", "name", "category", "description"],
            ["calendar_events"] =
                ["id", "area_id", "title", "description", "start_at", "end_at"],
            ["notes"] =
                ["id", "area_id", "title", "content", "created_at", "last_modified_at"]
        };

        foreach (var table in expectedColumns)
        {
            if (!await TableExistsAsync(
                    connection,
                    table.Key,
                    cancellationToken))
            {
                continue;
            }

            var actualColumns = await GetTableColumnsAsync(
                connection,
                table.Key,
                cancellationToken);

            if (!actualColumns.SetEquals(table.Value))
            {
                throw CreateUnrecognizedLegacySchemaException(table.Key);
            }
        }

        if (!await TableExistsAsync(connection, "goals", cancellationToken))
        {
            return;
        }

        var goalColumns = await GetTableColumnsAsync(
            connection,
            "goals",
            cancellationToken);
        var originalGoalColumns = new HashSet<string>(
            ["id", "area_id", "name", "description"],
            StringComparer.OrdinalIgnoreCase);
        var completedGoalColumns = new HashSet<string>(originalGoalColumns,
            StringComparer.OrdinalIgnoreCase)
        {
            "is_completed"
        };

        if (!goalColumns.SetEquals(originalGoalColumns) &&
            !goalColumns.SetEquals(completedGoalColumns))
        {
            throw CreateUnrecognizedLegacySchemaException("goals");
        }
    }

    private static SqliteMigrationException
        CreateUnrecognizedLegacySchemaException(string table) =>
        new(
            0,
            "Recognize legacy database",
            new InvalidDataException(
                $"La tabla heredada '{table}' no coincide con ninguna " +
                "versión histórica conocida y no puede migrarse con seguridad."));

    private static async Task<HashSet<int>> GetAppliedVersionsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var versions = new HashSet<int>();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT version FROM schema_migrations ORDER BY version;";

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            versions.Add(reader.GetInt32(0));
        }

        return versions;
    }

    private void ValidateAppliedVersions(HashSet<int> appliedVersions)
    {
        if (appliedVersions.Count == 0)
        {
            return;
        }

        var databaseVersion = appliedVersions.Max();

        if (databaseVersion > CurrentSchemaVersion)
        {
            throw new SqliteSchemaVersionTooNewException(
                databaseVersion,
                CurrentSchemaVersion);
        }

        for (var version = 1; version <= databaseVersion; version++)
        {
            if (!appliedVersions.Contains(version))
            {
                throw new SqliteMigrationException(
                    version,
                    "Validate migration history",
                    new InvalidDataException(
                        $"Falta la migración registrada número '{version}'."));
            }
        }
    }

    private static async Task ApplyMigrationAsync(
        SqliteConnection connection,
        SqliteMigration migration,
        CancellationToken cancellationToken)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            await migration.ApplyAsync(
                connection,
                transaction,
                cancellationToken);

            await using var historyCommand = connection.CreateCommand();
            historyCommand.Transaction = transaction;
            historyCommand.CommandText =
                """
                INSERT INTO schema_migrations (version, name, applied_at)
                VALUES ($version, $name, $appliedAt);
                """;
            historyCommand.Parameters.AddWithValue(
                "$version",
                migration.Version);
            historyCommand.Parameters.AddWithValue(
                "$name",
                migration.Name);
            historyCommand.Parameters.AddWithValue(
                "$appliedAt",
                DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            await historyCommand.ExecuteNonQueryAsync(cancellationToken);

            transaction.Commit();
        }
        catch (Exception exception)
        {
            transaction.Rollback();

            if (exception is OperationCanceledException)
            {
                throw;
            }

            if (exception is SqliteException sqliteException)
            {
                var classified = ClassifySqliteException(sqliteException);

                if (classified is SqliteDatabaseCorruptException or
                    SqliteDatabaseLockedException)
                {
                    throw classified;
                }
            }

            throw new SqliteMigrationException(
                migration.Version,
                migration.Name,
                exception);
        }
    }

    private static async Task EnsureDefaultAreaAsync(
        SqliteConnection connection,
        AreaId defaultAreaId,
        CancellationToken cancellationToken)
    {
        using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT OR IGNORE INTO areas (id)
            VALUES ($areaId);
            """;
        command.Parameters.AddWithValue(
            "$areaId",
            defaultAreaId.Value.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
        transaction.Commit();
    }

    private async Task ValidateSchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var taskColumns = new List<string>
        {
            "id",
            "area_id",
            "title",
            "description",
            "is_completed"
        };

        if (CurrentSchemaVersion >= 8)
        {
            taskColumns.Add("project_id");
        }

        var expectedSchema = new Dictionary<string, string[]>
        {
            ["schema_migrations"] = ["version", "name", "applied_at"],
            ["areas"] = ["id"],
            ["goals"] =
                ["id", "area_id", "name", "description", "is_completed"],
            ["projects"] = ["id", "area_id", "name", "description"],
            ["tasks"] = taskColumns.ToArray(),
            ["resources"] =
                ["id", "area_id", "name", "category", "description"],
            ["calendar_events"] =
                ["id", "area_id", "title", "description", "start_at", "end_at"],
            ["notes"] =
                ["id", "area_id", "title", "content", "created_at", "last_modified_at"]
        };

        foreach (var table in expectedSchema)
        {
            var actualColumns = await GetTableColumnsAsync(
                connection,
                table.Key,
                cancellationToken);

            if (!actualColumns.SetEquals(table.Value))
            {
                throw new SqliteMigrationException(
                    CurrentSchemaVersion,
                    "Validate database schema",
                    new InvalidDataException(
                        $"La tabla '{table.Key}' no coincide con el esquema esperado."));
            }
        }

        await using var foreignKeyCommand = connection.CreateCommand();
        foreignKeyCommand.CommandText = "PRAGMA foreign_key_check;";
        await using var foreignKeyReader =
            await foreignKeyCommand.ExecuteReaderAsync(cancellationToken);

        if (await foreignKeyReader.ReadAsync(cancellationToken))
        {
            throw new SqliteMigrationException(
                CurrentSchemaVersion,
                "Validate database integrity",
                new InvalidDataException(
                    "La base contiene referencias que incumplen sus claves foráneas."));
        }
    }

    private static async Task<bool> TableExistsAsync(
        SqliteConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table' AND name = $table;
            """;
        command.Parameters.AddWithValue("$table", table);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture) == 1;
    }

    private static async Task<HashSet<string>> GetTableColumnsAsync(
        SqliteConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{table}\");";

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private void CreateBackup(
        SqliteConnection sourceConnection,
        int migrationVersion)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString(
            "yyyyMMddHHmmssfff",
            CultureInfo.InvariantCulture);
        var backupPath =
            $"{_connectionFactory.DatabasePath}.backup-v{migrationVersion}-{timestamp}";

        var backupConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = backupPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        using var backupConnection =
            new SqliteConnection(backupConnectionString);
        backupConnection.Open();
        sourceConnection.BackupDatabase(backupConnection);
    }

    internal static SqliteDatabaseInitializationException
        ClassifySqliteException(SqliteException exception) =>
        exception.SqliteErrorCode switch
        {
            5 or 6 => new SqliteDatabaseLockedException(exception),
            11 or 26 => new SqliteDatabaseCorruptException(
                "SQLite indicó que la base de datos de Lorcaire " +
                "está corrupta o no tiene un formato válido.",
                exception),
            _ => new SqliteMigrationException(
                0,
                "Initialize database",
                exception)
        };
}
