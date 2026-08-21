using Lorcaire.Core.Domain.Areas;
using Lorcaire.Core.Domain.Notes;
using Lorcaire.Infrastructure.Persistence.Memory;

namespace Lorcaire.Infrastructure.Tests.Persistence.Memory;

public sealed class InMemoryNoteRepositoryTests
{
    [Fact]
    public async Task Repository_OrdersNotesByMostRecentlyModified()
    {
        var repository = new InMemoryNoteRepository();
        var areaId = AreaId.New();
        var now = DateTimeOffset.UtcNow;
        var older = new Note(NoteId.New(), areaId, "Older", "Content", now);
        var newer = new Note(
            NoteId.New(),
            areaId,
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
    public async Task Repository_UpdatesNote()
    {
        var repository = new InMemoryNoteRepository();
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            DateTimeOffset.UtcNow);
        await repository.AddAsync(note);
        note.Update("Updated", "New content", note.CreatedAt.AddMinutes(1));
        await repository.UpdateAsync(note);

        Assert.Equal(
            "Updated",
            (await repository.GetByIdAsync(note.Id))!.Title);
    }

    [Fact]
    public async Task Repository_RejectsDuplicateId()
    {
        var repository = new InMemoryNoteRepository();
        var note = new Note(
            NoteId.New(),
            AreaId.New(),
            "Title",
            "Content",
            DateTimeOffset.UtcNow);
        await repository.AddAsync(note);

        await Assert.ThrowsAsync<ConflictException>(
            () => repository.AddAsync(note));
    }
}
