using Lorcaire.Application.Goals.Persistence;
using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.DeleteGoal;

public sealed class DeleteGoalHandler(IGoalRepository goalRepository)
{
    public async Task HandleAsync(
        Guid goalId,
        CancellationToken cancellationToken = default)
    {
        var deleted = await goalRepository.DeleteAsync(
            new GoalId(goalId),
            cancellationToken);

        if (!deleted)
        {
            throw new GoalNotFoundException(goalId);
        }
    }
}
