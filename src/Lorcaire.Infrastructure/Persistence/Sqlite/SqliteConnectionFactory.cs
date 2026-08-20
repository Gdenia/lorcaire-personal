using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

public sealed class SqliteConnectionFactory
{
    private static readonly object InitializationLock = new();
    private static bool _providerInitialized;

    private readonly string _connectionString;

    internal string DatabasePath { get; }

    public SqliteConnectionFactory(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException(
                "La ruta de la base de datos es obligatoria.",
                nameof(databasePath));
        }

        EnsureProviderInitialized();

        var fullPath = Path.GetFullPath(databasePath);
        DatabasePath = fullPath;
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        };

        _connectionString = connectionString.ToString();
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    private static void EnsureProviderInitialized()
    {
        lock (InitializationLock)
        {
            if (_providerInitialized)
            {
                return;
            }

            SQLitePCL.Batteries_V2.Init();

            _providerInitialized = true;
        }
    }
}
