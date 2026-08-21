namespace Lorcaire.Core.Domain.Tasks;

public readonly record struct TaskId
{
    public Guid Value { get; }

    public TaskId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "The task identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public static TaskId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
