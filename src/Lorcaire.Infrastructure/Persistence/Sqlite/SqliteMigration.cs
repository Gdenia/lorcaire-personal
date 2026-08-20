using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

internal sealed class SqliteMigration
{
    private readonly Func<
        SqliteConnection,
        SqliteTransaction,
        CancellationToken,
        Task> _apply;

    public SqliteMigration(
        int version,
        string name,
        bool requiresBackup,
        Func<
            SqliteConnection,
            SqliteTransaction,
            CancellationToken,
            Task> apply)
    {
        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre de la migración es obligatorio.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(apply);

        Version = version;
        Name = name;
        RequiresBackup = requiresBackup;
        _apply = apply;
    }

    public int Version { get; }

    public string Name { get; }

    public bool RequiresBackup { get; }

    public Task ApplyAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken) =>
        _apply(connection, transaction, cancellationToken);

    public static SqliteMigration FromScript(
        int version,
        string name,
        string script,
        bool requiresBackup = false) =>
        new(
            version,
            name,
            requiresBackup,
            async (connection, transaction, cancellationToken) =>
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = script;
                await command.ExecuteNonQueryAsync(cancellationToken);
            });
}
