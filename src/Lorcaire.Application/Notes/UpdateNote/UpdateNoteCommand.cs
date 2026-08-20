namespace Lorcaire.Application.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    Guid NoteId,
    string Title,
    string Content);
