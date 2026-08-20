using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;
using Lorcaire.Infrastructure.Persistence.Sqlite;

namespace Lorcaire.Infrastructure.Tests.Persistence.Sqlite;

public sealed class SqliteNoteRepositoryTests
{
    [Fact] public async Task Repository_DeletesWithoutAffectingOtherNotes(){await using var database=await TemporaryDatabase.CreateAsync();var r=new SqliteNoteRepository(database.ConnectionFactory);var now=DateTimeOffset.UtcNow;var first=new Note(NoteId.New(),database.DefaultAreaId,"First","Body",now);var second=new Note(NoteId.New(),database.DefaultAreaId,"Second","Body",now);await r.AddAsync(first);await r.AddAsync(second);Assert.True(await r.DeleteAsync(first.Id));Assert.False(await r.DeleteAsync(first.Id));Assert.Equal(second.Id,(await r.GetByIdAsync(second.Id))!.Id);}
    [Fact]
    public async Task Repository_PersistsAndReadsNote()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var writer = new SqliteNoteRepository(database.ConnectionFactory);
        var createdAt = new DateTimeOffset(
            2026,
            8,
            21,
            10,
            0,
            0,
            TimeSpan.Zero);
        var note = new Note(
            NoteId.New(),
            database.DefaultAreaId,
            "Title",
            "Content",
            createdAt);
        await writer.AddAsync(note);

        var reader = new SqliteNoteRepository(database.ConnectionFactory);
        var stored = Assert.Single(await reader.GetAllAsync());

        Assert.Equal(note.Id, stored.Id);
        Assert.Equal(note.AreaId, stored.AreaId);
        Assert.Equal(note.Title, stored.Title);
        Assert.Equal(note.Content, stored.Content);
        Assert.Equal(note.CreatedAt, stored.CreatedAt);
        Assert.Equal(note.LastModifiedAt, stored.LastModifiedAt);
    }

    [Fact]
    public async Task Repository_PersistsUpdatesAcrossConnections()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteNoteRepository(database.ConnectionFactory);
        var note = new Note(
            NoteId.New(),
            database.DefaultAreaId,
            "Title",
            "Content",
            DateTimeOffset.UtcNow);
        await repository.AddAsync(note);
        note.Update("Updated", "New content", note.CreatedAt.AddHours(1));
        await repository.UpdateAsync(note);

        var reader = new SqliteNoteRepository(database.ConnectionFactory);
        var stored = await reader.GetByIdAsync(note.Id);

        Assert.NotNull(stored);
        Assert.Equal("Updated", stored.Title);
        Assert.Equal("New content", stored.Content);
        Assert.Equal(note.LastModifiedAt, stored.LastModifiedAt);
    }

    [Fact]
    public async Task Repository_OrdersByMostRecentlyModified()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteNoteRepository(database.ConnectionFactory);
        var now = DateTimeOffset.UtcNow;
        var older = new Note(
            NoteId.New(),
            database.DefaultAreaId,
            "Older",
            "Content",
            now);
        var newer = new Note(
            NoteId.New(),
            database.DefaultAreaId,
            "Newer",
            "Content",
            now,
            now.AddHours(1));
        await repository.AddAsync(older);
        await repository.AddAsync(newer);

        var notes = await repository.GetAllAsync();

        Assert.Collection(
            notes,
            note => Assert.Equal(newer.Id, note.Id),
            note => Assert.Equal(older.Id, note.Id));
    }

    [Fact]
    public async Task Repository_RejectsUnknownArea()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteNoteRepository(database.ConnectionFactory);
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(note));
    }

    [Fact]
    public async Task Repository_RejectsUpdateForMissingNote()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var repository = new SqliteNoteRepository(database.ConnectionFactory);
        var note = new Note(
            NoteId.New(),
            database.DefaultAreaId,
            "Title",
            "Content",
            DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateAsync(note));
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
