namespace Lorcaire.Application.Tasks.GetTasks;

public sealed record TaskSummary(
    Guid Id,
    Guid AreaId,
    string Title,
    string? Description,
    bool IsCompleted);
