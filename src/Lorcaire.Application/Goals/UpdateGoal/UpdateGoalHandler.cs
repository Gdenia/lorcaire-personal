using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.UpdateGoal;

public sealed class UpdateGoalHandler(IGoalRepository goalRepository)
{
    public async Task HandleAsync(
        UpdateGoalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var goalId = new GoalId(command.GoalId);
        var goal = await goalRepository.GetByIdAsync(goalId, cancellationToken)
            ?? throw new GoalNotFoundException(command.GoalId);

        goal.Rename(command.Name);
        goal.ChangeDescription(command.Description);

        await goalRepository.UpdateAsync(goal, cancellationToken);
    }
}
