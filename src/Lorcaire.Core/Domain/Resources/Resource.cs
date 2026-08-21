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
        DomainValidation.EnsureIdentifier(id.Value, "resource", nameof(id));
        DomainValidation.EnsureIdentifier(areaId.Value, "area", nameof(areaId));
        Id = id;
        AreaId = areaId;
        Name = ValidateName(name);
        Category = ValidateRequired(
            category,
            DomainTextLimits.CategoryMaximumLength,
            "resource category",
            nameof(category));
        Description = NormalizeDescription(description);
    }

    public void Rename(string name) =>
        Name = ValidateName(name);

    public void ChangeCategory(string category) =>
        Category = ValidateRequired(
            category,
            DomainTextLimits.CategoryMaximumLength,
            "resource category",
            nameof(category));

    public void ChangeDescription(string? description) =>
        Description = NormalizeDescription(description);

    public void UpdateDetails(
        string name,
        string category,
        string? description)
    {
        var validatedName = ValidateName(name);
        var validatedCategory = ValidateRequired(
            category,
            DomainTextLimits.CategoryMaximumLength,
            "resource category",
            nameof(category));
        var validatedDescription = NormalizeDescription(description);

        Name = validatedName;
        Category = validatedCategory;
        Description = validatedDescription;
    }

    private static string ValidateName(string name) =>
        ValidateRequired(
            name,
            DomainTextLimits.NameMaximumLength,
            "resource name",
            nameof(name));

    private static string ValidateRequired(
        string value,
        int maximumLength,
        string fieldName,
        string parameterName)
        => DomainValidation.RequiredText(
            value,
            maximumLength,
            fieldName,
            parameterName);

    private static string? NormalizeDescription(string? description) =>
        DomainValidation.OptionalText(
            description,
            DomainTextLimits.DescriptionMaximumLength,
            "resource description",
            nameof(description));
}
