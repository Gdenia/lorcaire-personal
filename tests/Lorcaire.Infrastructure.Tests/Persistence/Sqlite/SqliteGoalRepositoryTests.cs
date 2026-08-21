using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Goals;
using Lorcaire.Infrastructure.Persistence.Sqlite;
using Lorcaire.Application.Errors;
using Lorcaire.Application.Goals.UpdateGoal;
using Lorcaire.Core.Domain;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteGoalRepositoryTests
{
    [Fact]
    public async Task Repository_PersistsEditsAndCompletionState()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteGoalRepository(database.ConnectionFactory);
        var goal = new Goal(
            GoalId.New(), database.DefaultAreaId, "Original", "Before");
        await repository.AddAsync(goal);

        goal.Rename("Updated");
        goal.ChangeDescription("After");
        goal.Complete();
        await repository.UpdateAsync(goal);

        var stored = await new SqliteGoalRepository(database.ConnectionFactory)
            .GetByIdAsync(goal.Id);
        Assert.NotNull(stored);
        Assert.Equal("Updated", stored.Name);
        Assert.Equal("After", stored.Description);
        Assert.True(stored.IsCompleted);
    }

    [Fact]
    public async Task Repository_DeletesPersistedGoal()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteGoalRepository(database.ConnectionFactory);
        var goal = new Goal(GoalId.New(), database.DefaultAreaId, "Delete me");
        await repository.AddAsync(goal);

        Assert.True(await repository.DeleteAsync(goal.Id));
        Assert.False(await repository.DeleteAsync(goal.Id));
        Assert.Null(await repository.GetByIdAsync(goal.Id));
    }

    [Fact]
    public async Task Repository_RejectsUpdateForMissingGoal()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteGoalRepository(database.ConnectionFactory);
        var goal = new Goal(GoalId.New(), database.DefaultAreaId, "Missing");

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => repository.UpdateAsync(goal));

        Assert.DoesNotContain("SQLite", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FailedApplicationUpdate_PreservesPersistedGoal()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteGoalRepository(database.ConnectionFactory);
        var goal = new Goal(
            GoalId.New(),
            database.DefaultAreaId,
            "Original",
            "Before",
            isCompleted: true);
        await repository.AddAsync(goal);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new UpdateGoalHandler(repository).HandleAsync(new(
                goal.Id.Value,
                "Changed",
                new string(
                    'x',
                    DomainTextLimits.DescriptionMaximumLength + 1))));

        var stored = await repository.GetByIdAsync(goal.Id);
        Assert.NotNull(stored);
        Assert.Equal("Original", stored.Name);
        Assert.Equal("Before", stored.Description);
        Assert.True(stored.IsCompleted);
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
                Path.GetTempPath(), "Lorcaire.Tests", Guid.NewGuid().ToString("N"));
            var defaultAreaId = AreaId.New();
            var factory = new SqliteConnectionFactory(
                Path.Combine(directoryPath, "lorcaire-tests.db"));
            await new SqliteDatabaseInitializer(factory).InitializeAsync(defaultAreaId);
            return new TemporaryDatabase(directoryPath, defaultAreaId, factory);
        }

        public ValueTask DisposeAsync()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }
            return ValueTask.CompletedTask;
        }
    }
}
