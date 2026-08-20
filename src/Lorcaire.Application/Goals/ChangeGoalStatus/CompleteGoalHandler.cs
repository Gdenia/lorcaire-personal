using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.ChangeGoalStatus;

public sealed class CompleteGoalHandler(IGoalRepository goalRepository)
{
    public async Task HandleAsync(
        Guid goalId,
        CancellationToken cancellationToken = default)
    {
        var id = new GoalId(goalId);
        var goal = await goalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new GoalNotFoundException(goalId);

        goal.Complete();
        await goalRepository.UpdateAsync(goal, cancellationToken);
    }
}
