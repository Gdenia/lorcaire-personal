namespace Lorcaire.Application.Resources.GetResources;

public sealed record ResourceSummary(
    Guid Id,
    Guid AreaId,
    string Name,
    string Category,
    string? Description);
