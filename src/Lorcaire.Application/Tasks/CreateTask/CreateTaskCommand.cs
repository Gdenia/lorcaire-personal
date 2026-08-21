namespace Lorcaire.Application.Tasks.CreateTask;

public sealed record CreateTaskCommand(
    Guid AreaId,
    string Title,
    string? Description,
    Guid? ProjectId = null);
