namespace Lorcaire.Core.Domain.Notes;

public readonly record struct NoteId
{
    public Guid Value { get; }

    public NoteId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de la nota no puede estar vacío.",
                nameof(value));
        }

        Value = value;
    }

    public static NoteId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
