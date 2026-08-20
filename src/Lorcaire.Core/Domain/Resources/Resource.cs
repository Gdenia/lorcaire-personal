using Lorcaire.Core.Domain.Areas;

namespace Lorcaire.Core.Domain.Resources;

public sealed class Resource
{
    public ResourceId Id { get; }
    public AreaId AreaId { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Category { get; private set; }

    public Resource(
        ResourceId id,
        AreaId areaId,
        string name,
        string category,
        string? description = null)
    {
        Id = id;
        AreaId = areaId;
        Name = ValidateRequired(name, "El recurso debe tener un nombre.", nameof(name));
        Category = ValidateRequired(
            category,
            "El recurso debe tener una categoría.",
            nameof(category));
        Description = NormalizeDescription(description);
    }

    public void Rename(string name) =>
        Name = ValidateRequired(name, "El recurso debe tener un nombre.", nameof(name));

    public void ChangeCategory(string category) =>
        Category = ValidateRequired(
            category,
            "El recurso debe tener una categoría.",
            nameof(category));

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    private static string ValidateRequired(
        string value,
        string message,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
