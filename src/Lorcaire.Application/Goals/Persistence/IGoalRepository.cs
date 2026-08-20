using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.Persistence;

public interface IGoalRepository
{
    Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default);

    Task<Goal?> GetByIdAsync(
        GoalId goalId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Goal goal,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        GoalId goalId,
        CancellationToken cancellationToken = default);
}
