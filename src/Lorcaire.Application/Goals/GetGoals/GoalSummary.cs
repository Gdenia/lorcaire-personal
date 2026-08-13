namespace Lorcaire.Application.Goals.GetGoals;

public sealed record GoalSummary(
    Guid Id,
    Guid AreaId,
    string Name,
    string? Description,
    bool IsCompleted);
