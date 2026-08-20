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
        Id = id;
        AreaId = areaId;
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El proyecto debe tener un nombre.",
                nameof(name));
        }

        return name.Trim();
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
