namespace Lorcaire.Core.Domain.Calendar;

public readonly record struct CalendarEventId
{
    public Guid Value { get; }

    public CalendarEventId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del evento no puede estar vacío.",
                nameof(value));
        }

        Value = value;
    }

    public static CalendarEventId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
