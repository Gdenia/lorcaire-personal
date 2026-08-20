namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public abstract class SqliteDatabaseInitializationException : Exception
{
    protected SqliteDatabaseInitializationException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class SqliteSchemaVersionTooNewException
    : SqliteDatabaseInitializationException
{
    public SqliteSchemaVersionTooNewException(
        int databaseVersion,
        int supportedVersion)
        : base(
            $"La base de datos usa la versión de esquema '{databaseVersion}', " +
            $"pero esta versión de Lorcaire solo admite hasta la '{supportedVersion}'.")
    {
        DatabaseVersion = databaseVersion;
        SupportedVersion = supportedVersion;
    }

    public int DatabaseVersion { get; }

    public int SupportedVersion { get; }
}

public sealed class SqliteMigrationException
    : SqliteDatabaseInitializationException
{
    public SqliteMigrationException(
        int version,
        string migrationName,
        Exception innerException)
        : base(
            $"No se pudo aplicar la migración SQLite {version}: {migrationName}.",
            innerException)
    {
        Version = version;
        MigrationName = migrationName;
    }

    public int Version { get; }

    public string MigrationName { get; }
}

public sealed class SqliteDatabaseCorruptException
    : SqliteDatabaseInitializationException
{
    public SqliteDatabaseCorruptException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class SqliteDatabaseLockedException
    : SqliteDatabaseInitializationException
{
    public SqliteDatabaseLockedException(Exception innerException)
        : base(
            "La base de datos de Lorcaire está bloqueada por otro proceso u operación.",
            innerException)
    {
    }
}
