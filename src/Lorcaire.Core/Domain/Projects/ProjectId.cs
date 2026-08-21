namespace Lorcaire.Core.Domain.Projects;

public readonly record struct ProjectId
{
    public Guid Value { get; }

    public ProjectId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "The project identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    public static ProjectId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
