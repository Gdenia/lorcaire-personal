namespace Lorcaire.Application.Tasks.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    Guid? ProjectId = null);
