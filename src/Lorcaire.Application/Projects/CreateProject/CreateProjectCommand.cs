namespace Lorcaire.Application.Projects.CreateProject;

public sealed record CreateProjectCommand(
    Guid AreaId,
    string Name,
    string? Description);
