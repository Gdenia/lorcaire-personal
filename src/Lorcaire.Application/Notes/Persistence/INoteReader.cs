using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Notes.Persistence;

public interface INoteReader
{
    Task<IReadOnlyList<Note>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
