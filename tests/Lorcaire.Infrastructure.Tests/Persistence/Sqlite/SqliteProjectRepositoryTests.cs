using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Projects;
using Lorcaire.Infrastructure.Persistence.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteProjectRepositoryTests
{
    [Fact]
    public async Task ProjectRepository_PersistsAndReadsProject()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteProjectRepository(database.ConnectionFactory);
        var project = new Project(
            ProjectId.New(),
            database.DefaultAreaId,
            "Proyecto persistente",
            "Debe sobrevivir a otra conexión.");

        await repository.AddAsync(project);
        var reader = new SqliteProjectRepository(database.ConnectionFactory);
        var stored = Assert.Single(await reader.GetAllAsync());

        Assert.Equal(project.Id, stored.Id);
        Assert.Equal(project.AreaId, stored.AreaId);
        Assert.Equal(project.Name, stored.Name);
        Assert.Equal(project.Description, stored.Description);
    }

    [Fact]
    public async Task ProjectRepository_RejectsUnknownArea()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteProjectRepository(database.ConnectionFactory);
        var project = new Project(ProjectId.New(), AreaId.New(), "Proyecto");

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.AddAsync(project));
    }

    [Fact]
    public async Task ProjectRepository_RejectsDuplicatedProjectId()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteProjectRepository(database.ConnectionFactory);
        var project = new Project(
            ProjectId.New(),
            database.DefaultAreaId,
            "Proyecto");
        await repository.AddAsync(project);

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.AddAsync(project));
    }

    [Fact] public async Task ProjectRepository_UpdatesAndDeletes()
    { await using var database=await TemporaryDatabase.CreateAsync(); var r=new SqliteProjectRepository(database.ConnectionFactory); var p=new Project(ProjectId.New(),database.DefaultAreaId,"Old"); await r.AddAsync(p); p.Rename("New"); await r.UpdateAsync(p); Assert.Equal("New",(await r.GetByIdAsync(p.Id))!.Name); Assert.True(await r.DeleteAsync(p.Id)); Assert.False(await r.DeleteAsync(p.Id)); }

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
