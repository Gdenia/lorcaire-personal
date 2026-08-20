using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Tasks;
using Lorcaire.Infrastructure.Persistence.Sqlite;
using DomainTask = Lorcaire.Core.Domain.Tasks.Task;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteTaskRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task Repository_PersistsAndReadsTask()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var writer = new SqliteTaskRepository(database.ConnectionFactory);
        var task = new DomainTask(
            TaskId.New(),
            database.DefaultAreaId,
            "Tarea persistente",
            "Descripción");
        await writer.AddAsync(task);

        var reader = new SqliteTaskRepository(database.ConnectionFactory);
        var stored = Assert.Single(await reader.GetAllAsync());

        Assert.Equal(task.Id, stored.Id);
        Assert.Equal(task.AreaId, stored.AreaId);
        Assert.Equal(task.Title, stored.Title);
        Assert.Equal(task.Description, stored.Description);
        Assert.False(stored.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_PersistsCompletionChanges()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteTaskRepository(database.ConnectionFactory);
        var task = new DomainTask(
            TaskId.New(),
            database.DefaultAreaId,
            "Tarea");
        await repository.AddAsync(task);
        task.Complete();
        await repository.UpdateAsync(task);

        var completed = await repository.GetByIdAsync(task.Id);
        Assert.NotNull(completed);
        Assert.True(completed.IsCompleted);

        completed.Reopen();
        await repository.UpdateAsync(completed);
        Assert.False((await repository.GetByIdAsync(task.Id))!.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_RejectsUnknownArea()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteTaskRepository(database.ConnectionFactory);
        var task = new DomainTask(TaskId.New(), AreaId.New(), "Tarea");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(task));
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_RejectsDuplicateId()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteTaskRepository(database.ConnectionFactory);
        var task = new DomainTask(
            TaskId.New(),
            database.DefaultAreaId,
            "Tarea");
        await repository.AddAsync(task);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(task));
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_RejectsUpdateForMissingTask()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteTaskRepository(database.ConnectionFactory);
        var task = new DomainTask(
            TaskId.New(),
            database.DefaultAreaId,
            "Tarea");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateAsync(task));
    }

    [Fact]
    public async System.Threading.Tasks.Task Repository_UpdatesWithoutChangingStatus_AndDeletes()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteTaskRepository(database.ConnectionFactory);
        var task = new DomainTask(TaskId.New(), database.DefaultAreaId, "Old", isCompleted: true);
        await repository.AddAsync(task);
        task.Rename("New");
        await repository.UpdateAsync(task);
        var stored = await repository.GetByIdAsync(task.Id);
        Assert.Equal("New", stored!.Title);
        Assert.True(stored.IsCompleted);
        Assert.True(await repository.DeleteAsync(task.Id));
        Assert.False(await repository.DeleteAsync(task.Id));
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

        public static async System.Threading.Tasks.Task<TemporaryDatabase>
            CreateAsync()
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Lorcaire.Tests",
                Guid.NewGuid().ToString("N"));
            var defaultAreaId = AreaId.New();
            var factory = new SqliteConnectionFactory(
                Path.Combine(directoryPath, "lorcaire-tests.db"));
            await new SqliteDatabaseInitializer(factory)
                .InitializeAsync(defaultAreaId);
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
