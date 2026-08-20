namespace Lorcaire.Application.Notes.UpdateNote;

public sealed class NoteNotFoundException : Exception
{
    public Guid NoteId { get; }

    public NoteNotFoundException(Guid noteId)
        : base($"No existe la nota con identificador '{noteId}'.")
    {
        NoteId = noteId;
    }
}
