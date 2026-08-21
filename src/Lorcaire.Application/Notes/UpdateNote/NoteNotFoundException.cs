using Lorcaire.Application.Errors;

namespace Lorcaire.Application.Notes.UpdateNote;

public sealed class NoteNotFoundException : NotFoundException
{
    public Guid NoteId { get; }

    public NoteNotFoundException(Guid noteId)
        : base($"No note exists with identifier '{noteId}'.")
    {
        NoteId = noteId;
    }
}
