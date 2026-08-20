using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Resources;
using Lorcaire.Infrastructure.Persistence.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteResourceRepositoryTests
{
    [Fact]
    public async Task Repository_PersistsAndReadsResource()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var writer = new SqliteResourceRepository(database.ConnectionFactory);
        var resource = new Resource(
            ResourceId.New(),
            database.DefaultAreaId,
            "Clean Architecture",
            "Book",
            "Reference");
        await writer.AddAsync(resource);

        var reader = new SqliteResourceRepository(database.ConnectionFactory);
        var stored = Assert.Single(await reader.GetAllAsync());

        Assert.Equal(resource.Id, stored.Id);
        Assert.Equal(resource.AreaId, stored.AreaId);
        Assert.Equal(resource.Name, stored.Name);
        Assert.Equal(resource.Category, stored.Category);
        Assert.Equal(resource.Description, stored.Description);
    }

    [Fact]
    public async Task Repository_RejectsUnknownArea()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteResourceRepository(database.ConnectionFactory);
        var resource = new Resource(
            ResourceId.New(),
            AreaId.New(),
            "Resource",
            "Book");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(resource));
    }

    [Fact]
    public async Task Repository_RejectsDuplicateId()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteResourceRepository(database.ConnectionFactory);
        var resource = new Resource(
            ResourceId.New(),
            database.DefaultAreaId,
            "Resource",
            "Book");
        await repository.AddAsync(resource);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(resource));
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
