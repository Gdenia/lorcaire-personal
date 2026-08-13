namespace Lorcaire.Application.Goals.CreateGoal;

public sealed record CreateGoalCommand(
    Guid AreaId,
    string Name,
    string? Description);
