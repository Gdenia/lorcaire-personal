using Lorcaire.Application.Goals.Persistence;

namespace Lorcaire.Application.Goals.GetGoals;

public sealed class GetGoalsHandler
{
    private readonly IGoalReader _goalReader;

    public GetGoalsHandler(IGoalReader goalReader)
    {
        _goalReader = goalReader;
    }

    public async Task<IReadOnlyList<GoalSummary>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var goals = await _goalReader.GetAllAsync(
            cancellationToken);

        return goals
            .Select(goal => new GoalSummary(
                goal.Id.Value,
                goal.AreaId.Value,
                goal.Name,
                goal.Description))
            .ToArray();
    }
}
