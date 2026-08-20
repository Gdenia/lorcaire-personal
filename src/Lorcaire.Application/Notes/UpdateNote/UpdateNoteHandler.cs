using Lorcaire.Application.Notes.Persistence;
using Lorcaire.Core.Domain.Notes;

namespace Lorcaire.Application.Notes.UpdateNote;

public sealed class UpdateNoteHandler
{
    private readonly INoteRepository _noteRepository;
    private readonly TimeProvider _timeProvider;

    public UpdateNoteHandler(
        INoteRepository noteRepository,
        TimeProvider timeProvider)
    {
        _noteRepository = noteRepository;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(
        UpdateNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var noteId = new NoteId(command.NoteId);
        var note = await _noteRepository.GetByIdAsync(noteId, cancellationToken)
            ?? throw new NoteNotFoundException(command.NoteId);

        note.Update(
            command.Title,
            command.Content,
            _timeProvider.GetUtcNow());
        await _noteRepository.UpdateAsync(note, cancellationToken);
    }
}
