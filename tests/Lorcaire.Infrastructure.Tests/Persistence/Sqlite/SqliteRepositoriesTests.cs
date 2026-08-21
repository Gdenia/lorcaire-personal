using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Infrastructure.Persistence.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteRepositoriesTests
{
    [Fact]
    public async Task Initializer_CreatesDefaultArea()
    {
        await using var database =
            await TemporaryDatabase.CreateAsync();

        var repository = new SqliteAreaRepository(
            database.ConnectionFactory);

        var exists = await repository.ExistsAsync(
            database.DefaultAreaId);

        Assert.True(exists);
    }

    [Fact]
    public async Task GoalRepository_PersistsAndReadsGoal()
    {
        await using var database =
            await TemporaryDatabase.CreateAsync();

        var writer = new SqliteGoalRepository(
            database.ConnectionFactory);

        var goal = new Goal(
            GoalId.New(),
            database.DefaultAreaId,
            "Objetivo persistente",
            "Este objetivo debe sobrevivir a otra conexión.");

        await writer.AddAsync(goal);

        var reader = new SqliteGoalRepository(
            database.ConnectionFactory);

        var storedGoals = await reader.GetAllAsync();
        var storedGoal = Assert.Single(storedGoals);

        Assert.Equal(goal.Id, storedGoal.Id);
        Assert.Equal(goal.AreaId, storedGoal.AreaId);
        Assert.Equal(goal.Name, storedGoal.Name);
        Assert.Equal(goal.Description, storedGoal.Description);
    }

    [Fact]
    public async Task GoalRepository_RejectsUnknownArea()
    {
        await using var database =
            await TemporaryDatabase.CreateAsync();

        var repository = new SqliteGoalRepository(
            database.ConnectionFactory);

        var goal = new Goal(
            GoalId.New(),
            AreaId.New(),
            "Objetivo sin área existente");

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.AddAsync(goal));
    }

    [Fact]
    public async Task GoalRepository_RejectsDuplicatedGoalId()
    {
        await using var database =
            await TemporaryDatabase.CreateAsync();

        var repository = new SqliteGoalRepository(
            database.ConnectionFactory);

        var goal = new Goal(
            GoalId.New(),
            database.DefaultAreaId,
            "Objetivo único");

        await repository.AddAsync(goal);

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.AddAsync(goal));
    }

    private sealed class TemporaryDatabase : IAsyncDisposable
    {
        private readonly string _directoryPath;

        public AreaId DefaultAreaId { get; }

        public SqliteConnectionFactory ConnectionFactory { get; }

        private TemporaryDatabase(
            string directoryPath,
            AreaId defaultAreaId,
            SqliteConnectionFactory connectionFactory)
        {
            _directoryPath = directoryPath;
            DefaultAreaId = defaultAreaId;
            ConnectionFactory = connectionFactory;
        }

        public static async Task<TemporaryDatabase> CreateAsync()
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Lorcaire.Tests",
                Guid.NewGuid().ToString("N"));

            var databasePath = Path.Combine(
                directoryPath,
                "lorcaire-tests.db");

            var defaultAreaId = AreaId.New();

            var connectionFactory =
                new SqliteConnectionFactory(databasePath);

            var initializer =
                new SqliteDatabaseInitializer(connectionFactory);

            await initializer.InitializeAsync(defaultAreaId);

            return new TemporaryDatabase(
                directoryPath,
                defaultAreaId,
                connectionFactory);
        }

        public ValueTask DisposeAsync()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(
                    _directoryPath,
                    recursive: true);
            }

            return ValueTask.CompletedTask;
        }
    }
}
