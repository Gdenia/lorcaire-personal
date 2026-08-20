namespace Lorcaire.Application.Resources.UpdateResource;
public sealed record UpdateResourceCommand(Guid ResourceId,string Name,string Category,string? Description);
