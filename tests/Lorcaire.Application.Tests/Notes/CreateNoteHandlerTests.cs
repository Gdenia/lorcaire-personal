using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Application.Notes.CreateNote;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Tests.Notes;

public sealed class CreateNoteHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesNote_UsingCurrentTime()
    {
        var now = new DateTimeOffset(2026, 8, 21, 10, 0, 0, TimeSpan.Zero);
        var repository = new FakeNoteRepository();
        var areaId = Guid.NewGuid();
        var handler = new CreateNoteHandler(
            new FakeAreaRepository(true),
            repository,
            new TestTimeProvider(now));

        var result = await handler.HandleAsync(
            new CreateNoteCommand(areaId, "Title", "Content"));

        var note = Assert.Single(repository.Notes);
        Assert.Equal(result.NoteId, note.Id.Value);
        Assert.Equal(areaId, note.AreaId.Value);
        Assert.Equal(now, note.CreatedAt);
        Assert.Equal(now, note.LastModifiedAt);
    }

    [Fact]
    public async Task HandleAsync_RejectsUnknownArea()
    {
        var repository = new FakeNoteRepository();
        var handler = new CreateNoteHandler(
            new FakeAreaRepository(false),
            repository,
            TimeProvider.System);

        await Assert.ThrowsAsync<AreaNotFoundException>(() =>
            handler.HandleAsync(
                new CreateNoteCommand(Guid.NewGuid(), "Title", "Content")));
        Assert.Empty(repository.Notes);
    }

    [Fact]
    public async Task HandleAsync_RejectsInvalidContent()
    {
        var repository = new FakeNoteRepository();
        var handler = new CreateNoteHandler(
            new FakeAreaRepository(true),
            repository,
            TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(
                new CreateNoteCommand(Guid.NewGuid(), "Title", " ")));
        Assert.Empty(repository.Notes);
    }

    private sealed class FakeAreaRepository(bool exists) : IAreaRepository
    {
        public Task<bool> ExistsAsync(
            AreaId areaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);
    }

    private sealed class FakeNoteRepository : INoteRepository
    {
        public List<Note> Notes { get; } = [];

        public Task AddAsync(
            Note note,
            CancellationToken cancellationToken = default)
        {
            Notes.Add(note);
            return Task.CompletedTask;
        }

        public Task<Note?> GetByIdAsync(
            NoteId noteId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Notes.SingleOrDefault(note => note.Id == noteId));

        public Task UpdateAsync(
            Note note,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task<bool> DeleteAsync(NoteId id,CancellationToken c=default)=>Task.FromResult(false);
    }

    private sealed class TestTimeProvider(
        DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
