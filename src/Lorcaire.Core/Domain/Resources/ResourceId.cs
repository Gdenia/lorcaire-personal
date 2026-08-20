namespace Lorcaire.Core.Domain.Resources;

public readonly record struct ResourceId
{
    public Guid Value { get; }

    public ResourceId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del recurso no puede estar vacío.",
                nameof(value));
        }

        Value = value;
    }

    public static ResourceId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
