using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Notes.Persistence;

public interface INoteRepository
{
    Task AddAsync(
        Note note,
        CancellationToken cancellationToken = default);

    Task<Note?> GetByIdAsync(
        NoteId noteId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Note note,
        CancellationToken cancellationToken = default);
}
