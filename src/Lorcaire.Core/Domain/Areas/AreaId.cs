namespace Lorcaire.Core.Domain.Areas;

public readonly record struct AreaId
{
    public Guid Value { get; }

    public AreaId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "The area identifier cannot be empty.",
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
