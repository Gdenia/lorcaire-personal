using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Core.Domain.Projects;

public sealed class Project
{
    public ProjectId Id { get; }
    public AreaId AreaId { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }

    public Project(
        ProjectId id,
        AreaId areaId,
        string name,
        string? description = null)
    {
        DomainValidation.EnsureIdentifier(id.Value, "project", nameof(id));
        DomainValidation.EnsureIdentifier(areaId.Value, "area", nameof(areaId));
        Id = id;
        AreaId = areaId;
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    public void UpdateDetails(string name, string? description)
    {
        var validatedName = ValidateName(name);
        var validatedDescription = NormalizeDescription(description);

        Name = validatedName;
        Description = validatedDescription;
    }

    private static string ValidateName(string name)
    {
        return DomainValidation.RequiredText(
            name,
            DomainTextLimits.NameMaximumLength,
            "project name",
            nameof(name));
    }

    private static string? NormalizeDescription(string? description) =>
        DomainValidation.OptionalText(
            description,
            DomainTextLimits.DescriptionMaximumLength,
            "project description",
            nameof(description));
}
