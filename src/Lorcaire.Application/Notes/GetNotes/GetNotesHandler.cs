using Lorcaire.Application.Notes.Persistence;

namespace Lorcaire.Application.Notes.GetNotes;

public sealed class GetNotesHandler
{
    private readonly INoteReader _noteReader;

    public GetNotesHandler(INoteReader noteReader) => _noteReader = noteReader;

    public async Task<IReadOnlyList<NoteSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var notes = await _noteReader.GetAllAsync(cancellationToken);

        return notes
            .Select(note => new NoteSummary(
                note.Id.Value,
                note.AreaId.Value,
                note.Title,
                note.Content,
                note.CreatedAt,
                note.LastModifiedAt))
            .ToArray();
    }
}
