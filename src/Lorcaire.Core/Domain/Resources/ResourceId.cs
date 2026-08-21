namespace Lorcaire.Core.Domain.Resources;

public readonly record struct ResourceId
{
    public Guid Value { get; }

    public ResourceId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "The resource identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public static ResourceId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
