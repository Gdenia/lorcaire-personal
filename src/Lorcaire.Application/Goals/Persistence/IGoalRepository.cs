using Lorcaire.Core.Domain.Goals;

namespace Lorcaire.Application.Goals.Persistence;

public interface IGoalRepository
{
    Task AddAsync(
        Goal goal,
        CancellationToken cancellationToken = default);
}
