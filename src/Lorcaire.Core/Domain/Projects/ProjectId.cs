namespace Lorcaire.Core.Domain.Projects;

public readonly record struct ProjectId
{
    public Guid Value { get; }

    public ProjectId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del proyecto no puede estar vacío.",
                nameof(value));
        }

        Value = value;
    }

    public static ProjectId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
