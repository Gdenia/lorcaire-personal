namespace Lorcaire.Application.Goals.UpdateGoal;

public sealed record UpdateGoalCommand(
    Guid GoalId,
    string Name,
    string? Description);
