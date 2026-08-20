namespace Lorcaire.Application.Resources.CreateResource;

public sealed record CreateResourceCommand(
    Guid AreaId,
    string Name,
    string Category,
    string? Description);
