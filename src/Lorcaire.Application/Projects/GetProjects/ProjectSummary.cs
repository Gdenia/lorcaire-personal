namespace Lorcaire.Application.Projects.GetProjects;

public sealed record ProjectSummary(
    Guid Id,
    Guid AreaId,
    string Name,
    string? Description);
