using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Core.Domain.Goals;

public sealed class Goal
{
    public GoalId Id { get; }

    public AreaId AreaId { get; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsCompleted { get; private set; }

    public Goal(
        GoalId id,
        AreaId areaId,
        string name,
        string? description = null,
        bool isCompleted = false)
    {
        DomainValidation.EnsureIdentifier(id.Value, "goal", nameof(id));
        DomainValidation.EnsureIdentifier(areaId.Value, "area", nameof(areaId));
        Id = id;
        AreaId = areaId;
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
        IsCompleted = isCompleted;
    }

    public void Rename(string name)
    {
        Name = ValidateName(name);
    }

    public void ChangeDescription(string? description)
    {
        Description = NormalizeDescription(description);
    }

    public void UpdateDetails(string name, string? description)
    {
        var validatedName = ValidateName(name);
        var validatedDescription = NormalizeDescription(description);

        Name = validatedName;
        Description = validatedDescription;
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Reopen()
    {
        IsCompleted = false;
    }

    private static string ValidateName(string name)
    {
        return DomainValidation.RequiredText(
            name,
            DomainTextLimits.NameMaximumLength,
            "goal name",
            nameof(name));
    }

    private static string? NormalizeDescription(string? description)
    {
        return DomainValidation.OptionalText(
            description,
            DomainTextLimits.DescriptionMaximumLength,
            "goal description",
            nameof(description));
    }
}
