namespace Lorcaire.Core.Domain.Goals;

public readonly record struct GoalId
{
    public Guid Value { get; }

    public GoalId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "The goal identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public static GoalId New()
    {
        return new GoalId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
