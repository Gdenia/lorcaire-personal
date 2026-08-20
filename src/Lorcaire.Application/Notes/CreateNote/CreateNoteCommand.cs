namespace Lorcaire.Application.Notes.CreateNote;

public sealed record CreateNoteCommand(
    Guid AreaId,
    string Title,
    string Content);
