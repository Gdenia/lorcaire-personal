namespace Lorcaire.Core.Domain.Areas;

public readonly record struct AreaId
{
    public Guid Value { get; }

    public AreaId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del área no puede estar vacío.",
                nameof(value));
        }

        Value = value;
    }

    public static AreaId New()
    {
        return new AreaId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
