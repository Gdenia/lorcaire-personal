using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Application.Notes.UpdateNote;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Tests.Notes;

public sealed class UpdateNoteHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesNote_UsingCurrentTime()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var modifiedAt = createdAt.AddHours(1);
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            createdAt);
        var repository = new FakeNoteRepository(note);
        var handler = new UpdateNoteHandler(
            repository,
            new TestTimeProvider(modifiedAt));

        await handler.HandleAsync(
            new UpdateNoteCommand(note.Id.Value, "New title", "New content"));

        Assert.Equal("New title", note.Title);
        Assert.Equal("New content", note.Content);
        Assert.Equal(modifiedAt, note.LastModifiedAt);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task HandleAsync_RejectsUnknownNote()
    {
        var repository = new FakeNoteRepository(null);

        await Assert.ThrowsAsync<NoteNotFoundException>(() =>
            new UpdateNoteHandler(repository, TimeProvider.System)
                .HandleAsync(
                    new UpdateNoteCommand(
                        Guid.NewGuid(),
                        "Title",
                        "Content")));
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task HandleAsync_DoesNotPersistInvalidContent()
    {
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            DateTimeOffset.UtcNow);
        var repository = new FakeNoteRepository(note);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new UpdateNoteHandler(repository, TimeProvider.System)
                .HandleAsync(
                    new UpdateNoteCommand(note.Id.Value, "Title", " ")));
        Assert.Equal(0, repository.UpdateCount);
    }

    private sealed class FakeNoteRepository(Note? note) : INoteRepository
    {
        public int UpdateCount { get; private set; }

        public Task AddAsync(
            Note value,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Note?> GetByIdAsync(
            NoteId noteId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(note?.Id == noteId ? note : null);

        public Task UpdateAsync(
            Note value,
            CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestTimeProvider(
        DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
