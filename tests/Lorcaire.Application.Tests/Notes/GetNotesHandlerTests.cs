using Lorcaire.Application.Notes.GetNotes;
using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Tests.Notes;

public sealed class GetNotesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsNoteSummaries()
    {
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            DateTimeOffset.UtcNow);
        var handler = new GetNotesHandler(new FakeNoteReader([note]));

        var summary = Assert.Single(await handler.HandleAsync());

        Assert.Equal(note.Id.Value, summary.Id);
        Assert.Equal(note.AreaId.Value, summary.AreaId);
        Assert.Equal(note.Title, summary.Title);
        Assert.Equal(note.Content, summary.Content);
        Assert.Equal(note.CreatedAt, summary.CreatedAt);
        Assert.Equal(note.LastModifiedAt, summary.LastModifiedAt);
    }

    private sealed class FakeNoteReader(
        IReadOnlyList<Note> notes) : INoteReader
    {
        public Task<IReadOnlyList<Note>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(notes);
    }
}
